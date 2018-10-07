using System.Text;
using NuKeeper.Inspection.Report;

namespace NuKeeper.Inspection.Tests.Report
{
    public class TestReportWriter : IReportWriter
    {
        private StringBuilder _data = new StringBuilder();

        public TestReportWriter()
        {
        }

        public void Close()
        {
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
