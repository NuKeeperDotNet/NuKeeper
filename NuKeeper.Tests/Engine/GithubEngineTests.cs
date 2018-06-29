using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.Inspection.Files;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GithubEngineTests
    {
        [Test]
        public async Task SuperHappyFunPath()
        {
            var github = Substitute.For<IGithub>();
            var repoDiscovery = Substitute.For<IGithubRepositoryDiscovery>();
            var repoEngine = Substitute.For<IGithubRepositoryEngine>();
            var folders = Substitute.For<IFolderFactory>();

            github.GetCurrentUser().Returns(
                RepositoryBuilder.MakeUser("http://test.user.com"));

            var setings = new GithubAuthSettings(new Uri("http://foo.com"), "token123");

            var engine = new GithubEngine(github, repoDiscovery, repoEngine,
                setings,
                folders, new NullNuKeeperLogger());

            var count = await engine.Run();

            Assert.That(count, Is.EqualTo(0));
        }
    }
}
