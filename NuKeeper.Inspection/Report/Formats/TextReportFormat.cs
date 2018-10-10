using System.Collections.Generic;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report.Formats
{
    public class TextReportFormat : IReportFormat
    {
        private readonly IReportWriter _writer;

        public TextReportFormat(IReportWriter writer)
        {
            _writer = writer;
        }

        public void Write(string name, IReadOnlyCollection<PackageUpdateSet> updates)
        {
            _writer.WriteLine(MessageForCount(updates.Count));

            foreach (var update in updates)
            {
                _writer.WriteLine(Description.ForUpdateSet(update));
            }

            _writer.Close();
        }

        private string MessageForCount(int count)
        {
            if (count == 0)
            {
                return "Found no package updates";
            }
            if (count == 1)
            {
                return "Found 1 package update";
            }

            return $"Found {count} package updates";
        }
    }
}
