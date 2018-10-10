using System;

namespace NuKeeper.Inspection.Report
{
    public sealed class ConsoleReportWriter : IReportWriter
    {
        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }

        public void Dispose()
        {
        }
    }
}
