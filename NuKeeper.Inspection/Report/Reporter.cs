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
            string reportName,
            string fileName,
            IReadOnlyCollection<PackageUpdateSet> updates)
        {
            var destinationDesc = destination == OutputDestination.File ?
                $" File '{fileName}'":
                destination.ToString();

            _logger.Detailed($"Output report named {reportName}, is {format} to {destinationDesc}");

            using (var writer = MakeReportWriter(destination, fileName))
            {
                var reporter = MakeReporter(format, writer);
                reporter.Write(reportName, updates);
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
                    return new NullReportFormat();

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

        private static IReportWriter MakeReportWriter(
            OutputDestination destination,
            string fileName)
        {
            switch (destination)
            {
                case OutputDestination.Console:
                    return new ConsoleReportWriter();

                case OutputDestination.File:
                    return new FileReportWriter(fileName);

                case OutputDestination.Off:
                    return new NullReportWriter();

                default:
                    throw new ArgumentOutOfRangeException($"Invalid OutputDestination: {destination}");
            }
        }
    }
}
