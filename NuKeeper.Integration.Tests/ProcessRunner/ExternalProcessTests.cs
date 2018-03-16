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
            var result = await RunExternalProcess(DirCommand(), false);

            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.Output, Is.Not.Empty);
            Assert.That(result.Success, Is.True);
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
            Assert.ThrowsAsync<Exception>(() => RunExternalProcess(Guid.NewGuid().ToString("N"), true));
        }

        private static async Task<ProcessOutput> RunExternalProcess(string command, bool ensureSuccess)
        {
            IExternalProcess process;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process = new WindowsExternalProcess();
            }
            else
            {
                process = new UnixProcess();
            }

            return await process.Run(".", command, "", ensureSuccess);
        }

        private static string DirCommand()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "dir";
            }
            else
            {
                return "ls";
            }

        }
    }
}
