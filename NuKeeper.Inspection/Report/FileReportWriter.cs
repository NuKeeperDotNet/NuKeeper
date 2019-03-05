using System;
using System.IO;

namespace NuKeeper.Inspection.Report
{
    public class FileReportWriter : IReportWriter
    {
        private TextWriter _stream;

        public FileReportWriter(string fileName)
        {
            var output = new FileStream(fileName, FileMode.Create);
            _stream = new StreamWriter(output);
        }

        public void WriteLine(string value)
        {
            _stream.Write(value + Environment.NewLine);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream?.Dispose();
            }
            _stream = null;
        }
    }
}
