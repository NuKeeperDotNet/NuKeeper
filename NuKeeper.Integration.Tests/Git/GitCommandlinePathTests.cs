using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Git;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Git
{
    [TestFixture]
    public class GitCommandlinePathTests
    {
        [Test]
        public async Task HasGitPath()
        {
            var path = await new GitCommandlinePath(Substitute.For<INuKeeperLogger>()).Executable;

            Assert.That(path, Is.Not.Empty);
        }
    }
}
