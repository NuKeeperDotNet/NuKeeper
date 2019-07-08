using NuKeeper.Abstractions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NuKeeper.Integration.Tests.LogHelpers
{
    public class NuKeeperTestLogger : INuKeeperLogger
    {
        private readonly LogLevel _logLevel;

        private readonly List<string> _buffer = new List<string>();

        public NuKeeperTestLogger(LogLevel logLevel = LogLevel.Detailed)
        {
            _logLevel = logLevel;
        }

        public void ClearLog()
        {
            _buffer.Clear();
        }

        public void DumpLogToTestOutput()
        {
            var test = TestContext.CurrentContext.Test.Name;

            if (_buffer.Count > 0)
            {
                TestContext.Error.WriteLine($"{test}: NuKeeper Log:");
            }
            foreach (var line in _buffer)
            {
                TestContext.Error.WriteLine(line);
            }
            _buffer.Clear();
        }

        public void Error(string message, Exception ex = null)
        {
            Log(message, ex: ex);
        }

        public void Detailed(string message)
        {
            Log(message, LogLevel.Detailed);
        }

        public void Minimal(string message)
        {
            Log(message, LogLevel.Minimal);
        }

        public void Normal(string message)
        {
            Log(message, LogLevel.Normal);
        }

        private void Log(string message, LogLevel? level = null, Exception ex = null)
        {
            var test = TestContext.CurrentContext.Test.Name;

            if (_logLevel >= (level ?? LogLevel.Detailed))
            {
                var levelString = level?.ToString() ?? "Error";

                _buffer.Add($"{test}: {levelString} - {message}");

                if (ex != null)
                {
                    _buffer.Add($"{test}:   {ex.GetType().Name} : {ex.Message}");
                    foreach (var line in ex.StackTrace.Split(Environment.NewLine.ToCharArray()))
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            _buffer.Add($"{test}:     {line}");
                        }
                    }
                }
            }
        }
    }
}
