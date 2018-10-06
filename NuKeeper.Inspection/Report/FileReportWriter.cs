using System;
using System.IO;

namespace NuKeeper.Inspection.Report
{
    public class FileReportWriter: IReportWriter
    {
        private readonly TextWriter _stream;

        public FileReportWriter(string fileName)
        {
            var output = new FileStream(fileName, FileMode.Create);
            _stream = new StreamWriter(output);
        }

        public void WriteLine(string value)
        {
            _stream.Write(value + Environment.NewLine);
        }

        public void Close()
        {
            _stream.Flush();
            _stream.Close();
        }
    }
}
