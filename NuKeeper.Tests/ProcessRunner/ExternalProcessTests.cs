using System.Threading.Tasks;
using NuKeeper.ProcessRunner;
using NUnit.Framework;

namespace NuKeeper.Tests.ProcessRunner
{
    [TestFixture]
    public class ExternalProcessTests
    {
        [Test]
        public async Task SuccessCase()
        {
            var process = new ExternalProcess();
            var result = await process.Run("dir");

            Assert.That(result.ExitCode, Is.EqualTo(0));
            Assert.That(result.Output, Is.Not.Empty);
            Assert.That(result.Success, Is.True);
        }

        [Test]
        public async Task FailureCase()
        {
            var process = new ExternalProcess();
            var result = await process.Run("dirt");

            Assert.That(result.ExitCode, Is.Not.EqualTo(0));
            Assert.That(result.Success, Is.False);
        }
    }
}
