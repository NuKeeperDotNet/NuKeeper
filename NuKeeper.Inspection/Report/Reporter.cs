using System;
using System.Collections.Generic;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report.Formats;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public class Reporter: IReporter
    {
        private readonly INuKeeperLogger _logger;

        public Reporter(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Report(
            OutputDestination destination,
            OutputFormat format,
            string name,
            IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _logger.Detailed($"Output report mode is {format} to {destination}");

            using (var writer = MakeReportWriter(destination))
            {
                var reporter = MakeReporter(format, writer);
                reporter.Write(name, updates);
            }

            _logger.Detailed($"Wrote report for {updates.Count} updates");
        }

        private static IReportFormat MakeReporter(
            OutputFormat format,
            IReportWriter writer)
        {
            switch (format)
            {
                case OutputFormat.None:
                    return new NullReportFormat(writer);

                case OutputFormat.Text:
                    return new TextReportFormat(writer);

                case OutputFormat.Csv:
                    return new CsvReportFormat(writer);

                case OutputFormat.Metrics:
                    return new MetricsReportFormat(writer);

                case OutputFormat.LibYears:
                    return new LibYearsReportFormat(writer);

                default:
                    throw new ArgumentOutOfRangeException($"Invalid OutputFormat: {format}");
            }
        }

        private static IReportWriter MakeReportWriter(OutputDestination destination)
        {
            switch (destination)
            {
                case OutputDestination.Console:
                    return new ConsoleReportWriter();

                case OutputDestination.File:
                    return new FileReportWriter("nukeeper.out");

                case OutputDestination.Off:
                    return new NullReportWriter();

                default:
                    throw new ArgumentOutOfRangeException($"Invalid OutputDestination: {destination}");
            }
        }
    }
}
