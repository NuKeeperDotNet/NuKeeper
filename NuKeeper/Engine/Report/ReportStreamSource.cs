using NuKeeper.Types.Logging;
using System.IO;

namespace NuKeeper.Engine.Report
{
    public class ReportStreamSource: IReportStreamSource
    {
        private readonly INuKeeperLogger _logger;

        public ReportStreamSource(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public StreamWriter GetStream(string namePrefix)
        {
            var fileName = namePrefix + "_nukeeeper_report.csv";

            _logger.Verbose($"writing report to file at '{fileName}'");

            var output = new FileStream(fileName, FileMode.Create);
            return new StreamWriter(output);
        }
    }
}
