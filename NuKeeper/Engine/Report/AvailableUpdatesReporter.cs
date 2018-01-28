using NuKeeper.RepositoryInspection;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NuKeeper.Engine.Report
{
    public class AvailableUpdatesReporter: IAvailableUpdatesReporter
    {
        public void Report(List<PackageUpdateSet> updates)
        {
            using (var writer = MakeOutputStream())
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
            writer.WriteLine("Package id,Usage count,Versions in use,Lowest version in use, Highest Version in use,Highest available,Selected update,Package source");
        }

        private void WriteLine(StreamWriter writer, PackageUpdateSet update)
        {
            var occurences = update.CurrentPackages.Count;
            var versionsInUse = update.CurrentPackages
                .Select(p => p.Version);

            var lowest = versionsInUse.Min();
            var highest = versionsInUse.Max();

            writer.WriteLine($"{update.PackageId},{occurences},{update.CountCurrentVersions()},{lowest},{highest},{update.Highest},{update.NewPackage.Version},{update.PackageSource}");
        }

        private StreamWriter MakeOutputStream()
        {
            var output = new FileStream("nukeeeper_report.csv", FileMode.OpenOrCreate);
            return new StreamWriter(output);
        }
    }
}
