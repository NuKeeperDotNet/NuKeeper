using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using NUnit.Framework;

namespace NuKeeper.Update.Tests.Process
{
    [TestFixture]
    public class MonoExecutorTests
    {
        [TestCase(0, true)]
        [TestCase(1, false)]
        public async Task WhenCallingCanRun_ShouldCheckExternalProcessResult(int exitCode, bool expectedCanExecute)
        {
            var nuKeeperLogger = Substitute.For<INuKeeperLogger>();
            var externalProcess = Substitute.For<IExternalProcess>();

            externalProcess.Run("","mono","--version",false).
                Returns(new ProcessOutput("","",exitCode));

            var monoExecutor = new MonoExecutor(nuKeeperLogger, externalProcess);

            var canRun = await monoExecutor.CanRun();

            Assert.AreEqual(expectedCanExecute, canRun);
        }

        [Test]
        public async Task WhenCallingCanRun_ShouldOnlyCallExternalProcessOnce()
        {
            var nuKeeperLogger = Substitute.For<INuKeeperLogger>();
            var externalProcess = Substitute.For<IExternalProcess>();

            externalProcess.Run("","mono","--version",false).
                Returns(new ProcessOutput("","",0));

            var monoExecutor = new MonoExecutor(nuKeeperLogger, externalProcess);

            await monoExecutor.CanRun();
            await monoExecutor.CanRun();
            await monoExecutor.CanRun();

            await externalProcess.Received(1).Run(
                "",
                "mono",
                "--version",
                false);
        }

        [Test]
        public void WhenCallingRun_ShouldThrowIfMonoWasNotFound()
        {
            var nuKeeperLogger = Substitute.For<INuKeeperLogger>();
            var externalProcess = Substitute.For<IExternalProcess>();

            externalProcess.Run("","mono","--version",false).
                Returns(new ProcessOutput("","",1));

            var monoExecutor = new MonoExecutor(nuKeeperLogger, externalProcess);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await monoExecutor.Run("wd", "command", "args", true));
        }

        [Test]
        public async Task WhenCallingRun_ShouldPassArgumentToUnderlyingExternalProcess()
        {
            var nuKeeperLogger = Substitute.For<INuKeeperLogger>();
            var externalProcess = Substitute.For<IExternalProcess>();

            externalProcess.Run("","mono","--version",false).
                Returns(new ProcessOutput("","",0));

            var monoExecutor = new MonoExecutor(nuKeeperLogger, externalProcess);
            await monoExecutor.Run("wd", "command", "args", true);

            await externalProcess.Received(1).Run("wd", "mono", "command args", true);
        }
    }
}
