namespace NuKeeper.Inspection.Report
{
    public sealed class NullReportWriter : IReportWriter
    {
        public void WriteLine(string value)
        {
        }

        public void Dispose()
        {
        }
    }
}
