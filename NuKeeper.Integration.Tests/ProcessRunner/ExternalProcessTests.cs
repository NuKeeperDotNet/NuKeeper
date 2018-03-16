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
        public async Task InvalidCommandShouldThrowWhenSuccessIsEnsured()
        {
            Exception exThrown = null;
            try
            {
                await RunExternalProcess(Guid.NewGuid().ToString("N"), true);
            }
            catch (Exception ex)
            {
                exThrown = ex;
            }

            Assert.That(exThrown, Is.Not.Null);
        }

        private static async Task<ProcessOutput> RunExternalProcess(string command, bool ensureSuccess)
        {
            var process = OsExternalProcess();
            return await process.Run(".", command, "", ensureSuccess);
        }

        private static IExternalProcess OsExternalProcess()
        {
            if (IsWindows())
            {
                return new WindowsExternalProcess();
            }

            return new UnixProcess();
        }

        private static string DirCommand()
        {
            return IsWindows() ? "dir" : "ls";
        }

        private static bool IsWindows()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        }
    }
}
