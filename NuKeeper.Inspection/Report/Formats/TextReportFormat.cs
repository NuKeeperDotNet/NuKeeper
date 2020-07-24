using System;
using System.Collections.Generic;
using NuKeeper.Abstractions.RepositoryInspection;

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
            if (updates == null)
            {
                throw new ArgumentNullException(nameof(updates));
            }

            _writer.WriteLine(MessageForCount(updates.Count));

            foreach (var update in updates)
            {
                _writer.WriteLine(Description.ForUpdateSet(update));
            }
        }

        private static string MessageForCount(int count)
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
