using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.ProcessRunner
{
    [TestFixture]
    public class ExternalProcessTests
    {
        [Test]
        public async Task ValidCommandShouldSucceed()
        {
            var result = await RunExternalProcess("whoami", false);

            AssertSuccess(result);
        }

        [Test]
        public async Task DotNetCanRun()
        {
            var result = await RunExternalProcess("dotnet", "--version", true);

            AssertSuccess(result);
        }

        [Test]
        public async Task InvalidCommandShouldFail()
        {
            var result = await RunExternalProcess(Guid.NewGuid().ToString("N"), false);
        
            Assert.That(result.ExitCode, Is.Not.EqualTo(0));
            Assert.That(result.ErrorOutput, Is.Not.Empty);
            Assert.That(result.Success, Is.False);
        }

        [Test]
        public void InvalidCommandShouldThrowWhenSuccessIsEnsured()
        {
            Assert.ThrowsAsync(Is.AssignableTo<Exception>(),
                () => RunExternalProcess(Guid.NewGuid().ToString("N"), true));
        }

        private static async Task<ProcessOutput> RunExternalProcess(string command, bool ensureSuccess)
        {
            return await RunExternalProcess(command, "", ensureSuccess);
        }

        private static async Task<ProcessOutput> RunExternalProcess(string command, string args, bool ensureSuccess)
        {
            var process = ExternalProcess();
            return await process.Run(".", command, args, ensureSuccess);
        }

        private static IExternalProcess ExternalProcess()
        {
            return new ExternalProcess();
        }

        private static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }

        private static void AssertSuccess(ProcessOutput result)
        {
            Assert.That(result.ExitCode, Is.EqualTo(0), result.ErrorOutput);
            Assert.That(result.Success, Is.True);
            Assert.That(result.Output, Is.Not.Empty);
            Assert.That(result.ErrorOutput, Is.Empty);
        }
    }
}
