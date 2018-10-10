using System;

namespace NuKeeper.Inspection.Report
{
    public interface IReportWriter : IDisposable
    {
        void WriteLine(string value = "");
    }
}
