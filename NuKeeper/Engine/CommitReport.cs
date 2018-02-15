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
            return $"Automatic update of {updates.MatchId} to {updates.MatchVersion}";
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
 
            var newVersion = CodeQuote(updates.MatchVersion.ToString());
            var packageId = CodeQuote(updates.MatchId);

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

            var highestVersion = updates.HighestVersion;
            if (highestVersion != null && (highestVersion > updates.MatchVersion))
            {
                var allowedChange = CodeQuote(updates.AllowedChange.ToString());
                var highest = CodeQuote(updates.MatchId + " " + highestVersion);
                builder.AppendLine(
                    $"There is also a higher version, {highest}, but this was not applied as only {allowedChange} version changes are allowed.");
            }

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
                var line = $"Updated {CodeQuote(current.Path.RelativePath)} to {packageId} {CodeQuote(updates.MatchVersion.ToString())} from {CodeQuote(current.Version.ToString())}";
                builder.AppendLine(line);
            }

            builder.AppendLine("This is an automated update. Merge only if it passes tests");
            builder.AppendLine("");
            builder.AppendLine("**NuKeeper**: https://github.com/NuKeeperDotNet/NuKeeper");
            return builder.ToString();
        }

        private static string CodeQuote(string value)
        {
            return "`" + value + "`";
        }
    }
}
