using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Formats;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class CommitWording
    {
        private const string CommitEmoji = "package";

        public static string MakePullRequestTitle(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            if (updates.Count == 1)
            {
                return PackageTitle(updates.First());
            }

            return $"Automatic update of {updates.Count} packages";
        }

        private static string PackageTitle(PackageUpdateSet updates)
        {
            return $"Automatic update of {updates.SelectedId} to {updates.SelectedVersion}";
        }

        public static string MakeCommitMessage(PackageUpdateSet updates)
        {
            return $":{CommitEmoji}: {PackageTitle(updates)}";
        }

        public static string MakeCommitDetails(IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var builder = new StringBuilder();

            if (updates.Count > 1)
            {
                MultiPackagePrefix(updates, builder);
            }

            foreach (var update in updates)
            {
                builder.AppendLine(MakeCommitVersionDetails(update));
            }

            if (updates.Count > 1)
            {
                MultiPackageFooter(builder);
            }

            AddCommitFooter(builder);

            return builder.ToString();
        }

        private static void MultiPackagePrefix(IReadOnlyCollection<PackageUpdateSet> updates, StringBuilder builder)
        {
            var packageNames = updates
                .Select(p => CodeQuote(p.SelectedId))
                .JoinWithCommas();

            var projects = updates.SelectMany(
                    u => u.CurrentPackages)
                .Select(p => p.Path.FullName)
                .Distinct()
                .ToList();

            builder.AppendLine($"{updates.Count} packages were updated in {projects.Count} projects:");
            builder.AppendLine(packageNames);
            builder.AppendLine("<details>");
            builder.AppendLine("<summary>Details of updated packages</summary>");
            builder.AppendLine("");
        }

        private static void MultiPackageFooter(StringBuilder builder)
        {
            builder.AppendLine("</details>");
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

            if (updates.CurrentPackages.Count == 1)
            {
                builder.AppendLine("1 project update:");
            }
            else
            {
                builder.AppendLine($"{updates.CurrentPackages.Count} project updates:");
            }

            foreach (var current in updates.CurrentPackages)
            {
                var line = $"Updated {CodeQuote(current.Path.RelativePath)} to {packageId} {CodeQuote(updates.SelectedVersion.ToString())} from {CodeQuote(current.Version.ToString())}";
                builder.AppendLine(line);
            }

            if (SourceIsPublicNuget(updates.Selected.Source.SourceUri))
            {
                builder.AppendLine();
                builder.AppendLine(NugetPackageLink(updates.Selected.Identity));
            }

            return builder.ToString();
        }

        private static void AddCommitFooter(StringBuilder builder)
        {
            builder.AppendLine();
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

        private static string NugetPackageLink(PackageIdentity package)
        {
            var url = $"https://www.nuget.org/packages/{package.Id}/{package.Version}";
            return $"[{package.Id} {package.Version} on NuGet.org]({url})";
        }
    }
}
