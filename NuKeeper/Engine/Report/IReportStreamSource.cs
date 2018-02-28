using System.IO;

namespace NuKeeper.Engine.Report
{
    public interface IReportStreamSource
    {
        StreamWriter GetStream(string namePrefix);
    }
}
