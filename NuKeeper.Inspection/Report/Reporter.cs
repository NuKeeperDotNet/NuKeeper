using System;
using System.Collections.Generic;
using NuKeeper.Inspection.Logging;
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

            var reporter = MakeReporter(format, destination);
            reporter.Write(name, updates);

            _logger.Detailed($"Wrote report for {updates.Count} updates");
        }

        private IReportFormat MakeReporter(
            OutputFormat format,
            OutputDestination destination)
        {
            var target = MakeReportTarget(destination);

            switch (format)
            {
                case OutputFormat.Text:
                    return new TextReportFormat(target);

                case OutputFormat.Csv:
                    return new CsvReportFormat(target);

                default:
                    throw new ArgumentOutOfRangeException($"Invalid OutputFormat: {format}");
            }
        }

        private static IReportWriter MakeReportTarget(OutputDestination destination)
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
