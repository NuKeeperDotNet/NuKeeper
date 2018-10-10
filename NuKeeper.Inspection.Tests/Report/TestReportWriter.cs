using System.Text;
using NuKeeper.Inspection.Report;

namespace NuKeeper.Inspection.Tests.Report
{
    public class TestReportWriter : IReportWriter
    {
        private readonly StringBuilder _data = new StringBuilder();

        public int CloseCallCount { get; private set; }

        public TestReportWriter()
        {
        }

        public void Close()
        {
            CloseCallCount++;
        }

        public void WriteLine(string value = "")
        {
            _data.AppendLine(value);
        }

        public string Data()
        {
            return _data.ToString().TrimEnd();
        }
    }
}
