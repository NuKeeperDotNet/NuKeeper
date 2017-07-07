using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuKeeper.Nuget.Api;

namespace NuKeeper.Engine
{
    public class CommitReport
    {
        private readonly string _baseDir;

        public CommitReport(string baseDir)
        {
            _baseDir = baseDir;
        }

        public string MakeBranchName(string packageId, string oldVersionsString, PackageUpdate firstUpdate)
        {
            return $"nukeeper-update-{packageId}-from-{oldVersionsString}-to-{firstUpdate.NewVersion}";
        }

        public string MakeCommitMessage(List<PackageUpdate> updates)
        {
            return $"Automatic update of {updates[0].PackageId} to {updates[0].NewVersion}";
        }

        public string MakeCommitDetails(List<PackageUpdate> updates)
        {
            var oldVersions = updates
                .Select(u => CodeQuote(u.OldVersion.ToString()))
                .Distinct()
                .ToList();

            var oldVersionsString = string.Join(",", oldVersions);
            var newVersion = CodeQuote(updates[0].NewVersion.ToString());
            var packageId = CodeQuote(updates[0].PackageId);

            var builder = new StringBuilder();

            var headline = $"NuKeeper has generated an update of {packageId} from {oldVersionsString} to {newVersion}";
            builder.AppendLine(headline);

            if (oldVersions.Count > 1)
            {
                builder.AppendLine($"{oldVersions} versions were found in use: {oldVersionsString}");
            }

            if (updates.Count == 1)
            {
                builder.AppendLine("One project update:");
            }
            else
            {
                builder.AppendLine($"{updates.Count} project updates:");
            }

            foreach (var update in updates)
            {
                var relativePath = update.CurrentPackage.SourceFilePath.Replace(_baseDir, String.Empty);
                var line = $"Updated `{relativePath}` to {packageId} `{update.NewVersion}` from `{update.OldVersion}`";

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
