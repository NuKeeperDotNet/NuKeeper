using System;
using NuKeeper.RepositoryInspection;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NuKeeper.Engine.Report
{
    public class AvailableUpdatesReporter: IAvailableUpdatesReporter
    {
        public void Report(string name, List<PackageUpdateSet> updates)
        {
            using (var writer = MakeOutputStream(name))
            {
                WriteHeading(writer);
                foreach (var update in updates)
                {
                    WriteLine(writer, update);
                }

                writer.Close();
            }
        }

        private void WriteHeading(StreamWriter writer)
        {
            writer.WriteLine("Package id,Usage count,Versions in use,Lowest version in use,Highest Version in use,Highest available,Package source");
        }

        private void WriteLine(StreamWriter writer, PackageUpdateSet update)
        {
            var occurences = update.CurrentPackages.Count;
            var versionsInUse = update.CurrentPackages
                .Select(p => p.Version)
                .ToList();

            var lowest = versionsInUse.Min();
            var highest = versionsInUse.Max();

            writer.WriteLine($"{update.PackageId},{occurences},{update.CountCurrentVersions()},{lowest},{highest},{update.Highest},{update.PackageSource}");
        }

        private StreamWriter MakeOutputStream(string name)
        {
            var fileName = name + "_nukeeeper_report.csv";
            var output = new FileStream(fileName, FileMode.OpenOrCreate);
            return new StreamWriter(output);
        }

        private static string ToUtcIso8601(DateTimeOffset? source)
        {
            if (!source.HasValue)
            {
                return string.Empty;
            }

            const string iso8601Format = "yyyy-MM-ddTHH\\:mm\\:ss";
            var utcValue = source.Value.ToUniversalTime();
            return string.Concat(utcValue.ToString(iso8601Format, CultureInfo.InvariantCulture), "Z");
        }
    }
}
