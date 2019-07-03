using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsCommitWorder : ICommitWorder
    {
        private const string CommitEmoji = "📦";
    
        // Azure DevOps allows a maximum of 4000 characters to be used in a pull request description:
        // https://visualstudio.uservoice.com/forums/330519-azure-devops-formerly-visual-studio-team-services/suggestions/20217283-raise-the-character-limit-for-pull-request-descrip
        private const int MaxCharacterCount = 4000;

        public string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates.Count == 1)
            {
                return PackageTitle(updates.First());
            }

            return $"{CommitEmoji} Automatic update of {updates.Count} packages";
        }

        private static string PackageTitle(PackageUpdateSet updates)
        {
            return $"{CommitEmoji} Automatic update of {updates.SelectedId} to {updates.SelectedVersion}";
        }

        public string MakeCommitMessage(PackageUpdateSet updates)
        {
            return $"{PackageTitle(updates)}";
        }

        public string MakeCommitDetails(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var builder = new StringBuilder();

            if (updates.Count > 1)
            {
                MultiPackage(updates, builder);
            }

            foreach (var update in updates)
            {
                builder.AppendLine(MakeCommitVersionDetails(update));
            }

            AddCommitFooter(builder);

            if (builder.Length > MaxCharacterCount)
            {
                // Strip end of commit details since Azure DevOps can't handle a bigger pull request description.
                return $"{builder.ToString(0, MaxCharacterCount - 3)}...";
            }

            return builder.ToString();
        }

        private static void MultiPackage(IReadOnlyCollection<PackageUpdateSet> updates, StringBuilder builder)
        {
            var packageNames = updates
                .Select(p => p.SelectedId);

            var projects = updates.SelectMany(
                    u => u.CurrentPackages)
                .Select(p => p.Path.FullName)
                .Distinct()
                .ToList();

            var projectOptS = (projects.Count > 1) ? "s" : string.Empty;

            builder.AppendLine($"{updates.Count} packages were updated in {projects.Count} project{projectOptS}:");
            string updatedPackageNames = "|";
            foreach (var packageName in packageNames)
            {
                updatedPackageNames += $" {packageName} |";
            }

            builder.AppendLine(updatedPackageNames);
            builder.AppendLine("");
            builder.AppendLine("## Details of updated packages");
            builder.AppendLine("");
        }

        private static string MakeCommitVersionDetails(PackageUpdateSet updates)
        {
            var versionsInUse = updates.CurrentPackages
                .Select(u => u.Version)
                .Distinct()
                .ToList();

            var oldVersions = versionsInUse
                .Select(v => CodeQuote(v.ToString()))
                .ToList();

            var minOldVersion = versionsInUse.Min();

            var newVersion = CodeQuote(updates.SelectedVersion.ToString());
            var packageId = CodeQuote(updates.SelectedId);

            var changeLevel = ChangeLevel(minOldVersion, updates.SelectedVersion);

            var builder = new StringBuilder();

            if (oldVersions.Count == 1)
            {
                builder.AppendLine($"NuKeeper has generated a {changeLevel} update of {packageId} to {newVersion} from {oldVersions.JoinWithCommas()}");
            }
            else
            {
                builder.AppendLine($"NuKeeper has generated a {changeLevel} update of {packageId} to {newVersion}");
                builder.AppendLine($"{oldVersions.Count} versions of {packageId} were found in use: {oldVersions.JoinWithCommas()}");
            }

            if (updates.Selected.Published.HasValue)
            {
                var packageWithVersion = CodeQuote(updates.SelectedId + " " + updates.SelectedVersion);
                var pubDateString = CodeQuote(DateFormat.AsUtcIso8601(updates.Selected.Published));
                var pubDate = updates.Selected.Published.Value.UtcDateTime;
                var ago = TimeSpanFormat.Ago(pubDate, DateTime.UtcNow);

                builder.AppendLine($"{packageWithVersion} was published at {pubDateString}, {ago}");
            }

            var highestVersion = updates.Packages.Major?.Identity.Version;
            if (highestVersion != null && (highestVersion > updates.SelectedVersion))
            {
                LogHighestVersion(updates, highestVersion, builder);
            }

            builder.AppendLine();

            var updateOptS = (updates.CurrentPackages.Count > 1) ? "s" : string.Empty;
            builder.AppendLine($"### {updates.CurrentPackages.Count} project update{updateOptS}:");

            builder.AppendLine("| Project   | Package   | From   | To   |");
            builder.AppendLine("|:----------|:----------|-------:|-----:|");

            foreach (var current in updates.CurrentPackages)
            {
                string line;
                if (SourceIsPublicNuget(updates.Selected.Source.SourceUri))
                {
                    line = $"| {CodeQuote(current.Path.RelativePath)} | {CodeQuote(updates.SelectedId)} | {NuGetVersionPackageLink(current.Identity)} | {NuGetVersionPackageLink(updates.Selected.Identity)} |";
                    builder.AppendLine(line);

                    continue;
                }

                line = $"| {CodeQuote(current.Path.RelativePath)} | {CodeQuote(updates.SelectedId)} | {current.Version.ToString()} | {updates.SelectedVersion.ToString()} |";
                builder.AppendLine(line);
            }

            return builder.ToString();
        }

        private static void AddCommitFooter(StringBuilder builder)
        {
            builder.AppendLine("This is an automated update. Merge only if it passes tests");
            builder.AppendLine("**NuKeeper**: https://github.com/NuKeeperDotNet/NuKeeper");
        }

        private static string ChangeLevel(NuGetVersion oldVersion, NuGetVersion newVersion)
        {
            if (newVersion.Major > oldVersion.Major)
            {
                return "major";
            }

            if (newVersion.Minor > oldVersion.Minor)
            {
                return "minor";
            }

            if (newVersion.Patch > oldVersion.Patch)
            {
                return "patch";
            }

            if (!newVersion.IsPrerelease && oldVersion.IsPrerelease)
            {
                return "out of beta";
            }

            return string.Empty;
        }

        private static void LogHighestVersion(PackageUpdateSet updates, NuGetVersion highestVersion, StringBuilder builder)
        {
            var allowedChange = CodeQuote(updates.AllowedChange.ToString());
            var highest = CodeQuote(updates.SelectedId + " " + highestVersion);
            var highestPublishedAt = HighestPublishedAt(updates.Packages.Major.Published);

            builder.AppendLine(
                $"There is also a higher version, {highest}{highestPublishedAt}, " +
                $"but this was not applied as only {allowedChange} version changes are allowed.");
        }

        private static string HighestPublishedAt(DateTimeOffset? highestPublishedAt)
        {
            if (!highestPublishedAt.HasValue)
            {
                return string.Empty;
            }

            var highestPubDate = highestPublishedAt.Value;
            var formattedPubDate = CodeQuote(DateFormat.AsUtcIso8601(highestPubDate));
            var highestAgo = TimeSpanFormat.Ago(highestPubDate.UtcDateTime, DateTime.UtcNow);

            return $" published at {formattedPubDate}, {highestAgo}";
        }

        private static string CodeQuote(string value)
        {
            return "`" + value + "`";
        }

        private static bool SourceIsPublicNuget(Uri sourceUrl)
        {
            return
                sourceUrl != null &&
                sourceUrl.ToString().StartsWith("https://api.nuget.org/", StringComparison.OrdinalIgnoreCase);
        }

        private static string NuGetVersionPackageLink(PackageIdentity package)
        {
            var url = $"https://www.nuget.org/packages/{package.Id}/{package.Version}";
            return $"[{package.Version}]({url})";
        }
    }
}
