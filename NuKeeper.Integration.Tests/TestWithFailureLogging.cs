using NuGet.Common;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Integration.Tests.LogHelpers;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace NuKeeper.Integration.Tests
{
    public abstract class TestWithFailureLogging
    {
        private NuKeeperTestLogger _nkLogger = new NuKeeperTestLogger();
        private NugetTestLogger _ngLogger = new NugetTestLogger();

        public INuKeeperLogger NukeeperLogger => _nkLogger;
        public ILogger NugetLogger => _ngLogger;

        [TearDown]
        public void DumpLogWithError()
        {
            if (TestContext.CurrentContext.Result.Outcome != ResultState.Success)
            {
                _nkLogger.DumpLogToTestOutput();
                _ngLogger.DumpLogToTestOutput();
            }
            _nkLogger.ClearLog();
            _ngLogger.ClearLog();
        }
    }
}
