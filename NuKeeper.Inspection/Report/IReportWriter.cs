namespace NuKeeper.Inspection.Report
{
    public interface IReportWriter
    {
        void WriteLine(string value = "");
        void Close();
    }
}
