using System.Linq;
using System.Text;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class CommitReport
    {
        private const string CommitEmoji = "package";
        public static string MakePullRequestTitle(PackageUpdateSet updates)
        {
            return $"Automatic update of {updates.SelectedId} to {updates.SelectedVersion}";
        }

        public static string MakeCommitMessage(PackageUpdateSet updates)
        {
            return $":{CommitEmoji}: {MakePullRequestTitle(updates)}";
        }

        public static string MakeCommitDetails(PackageUpdateSet updates)
        {
            var oldVersions = updates.CurrentPackages
                .Select(u => u.Version)
                .Distinct()
                .Select(v => CodeQuote(v.ToString()))
                .ToList();
 
            var newVersion = CodeQuote(updates.SelectedVersion.ToString());
            var packageId = CodeQuote(updates.SelectedId);

            var builder = new StringBuilder();

            if (oldVersions.Count == 1)
            {
                builder.AppendLine($"NuKeeper has generated an update of {packageId} to {newVersion} from {oldVersions.JoinWithCommas()}");
            }
            else
            {
                builder.AppendLine($"NuKeeper has generated an update of {packageId} to {newVersion}");
                builder.AppendLine($"{oldVersions.Count} versions of {packageId} were found in use: {oldVersions.JoinWithCommas()}");
            }

            if (updates.Selected.Published.HasValue)
            {
                var packageWithVersion = CodeQuote(updates.SelectedId + " " + updates.SelectedVersion);
                var pubDate = CodeQuote(DateFormat.AsUtcIso8601(updates.Selected.Published));
                builder.AppendLine($"{packageWithVersion} was published at {pubDate}");
            }

            var highestVersion = updates.HighestVersion;
            if (highestVersion != null && (highestVersion > updates.SelectedVersion))
            {
                var allowedChange = CodeQuote(updates.AllowedChange.ToString());
                var highest = CodeQuote(updates.SelectedId + " " + highestVersion);
                string highestPublishedAt = string.Empty;
                if (updates.Packages.Major.Published.HasValue)
                {
                    highestPublishedAt = " published at " +
                        CodeQuote(DateFormat.AsUtcIso8601(updates.Packages.Major.Published));
                }
                builder.AppendLine(
                    $"There is also a higher version, {highest}{highestPublishedAt}, but this was not applied as only {allowedChange} version changes are allowed.");
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

            builder.AppendLine();
            builder.AppendLine("This is an automated update. Merge only if it passes tests");
            builder.AppendLine("**NuKeeper**: https://github.com/NuKeeperDotNet/NuKeeper");
            return builder.ToString();
        }

        private static string CodeQuote(string value)
        {
            return "`" + value + "`";
        }
    }
}
