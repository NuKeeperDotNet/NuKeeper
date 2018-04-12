using System.IO;

namespace NuKeeper.Inspection.Report
{
    public interface IReportStreamSource
    {
        StreamWriter GetStream(string namePrefix);
    }
}
