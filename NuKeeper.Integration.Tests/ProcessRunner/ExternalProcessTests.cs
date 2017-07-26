using System;
using System.Threading.Tasks;
using NuKeeper.ProcessRunner;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.ProcessRunner
{
    [TestFixture, Ignore("Windows only for now")]
    public class ExternalProcessTests
    {
        [Test]
        public async Task ValidCommandShouldSucceed()
        {
            var process = new ExternalProcess();
            var result = await process.Run("dir");

            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.Output, Is.Not.Empty);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public async Task InvalidCommandShouldFail()
        {
            var process = new ExternalProcess();
            var result = await process.Run(Guid.NewGuid().ToString("N"));

            Assert.That(result.ExitCode, Is.Not.EqualTo(0));
            Assert.That(result.Success, Is.False);
        }
    }
}
