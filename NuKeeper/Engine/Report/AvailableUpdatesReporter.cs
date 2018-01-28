using NuKeeper.RepositoryInspection;
using System.Collections.Generic;
using System.IO;

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
            writer.WriteLine("Package Id");
        }

        private void WriteLine(StreamWriter writer, PackageUpdateSet update)
        {
            writer.WriteLine(update.PackageId);
        }

        private StreamWriter MakeOutputStream()
        {
            var output = new FileStream("nukeeeper_report.csv", FileMode.OpenOrCreate);
            return new StreamWriter(output);
        }
    }
}
