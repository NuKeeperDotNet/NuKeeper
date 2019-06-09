using NuKeeper.Abstractions.Logging;
using NuKeeper.Git;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.Git.Tests
{
    public class GitCmdDiscoveryDriverTest
    {
        private INuKeeperLogger _logger;
        private string _pathTogit;
        private string _repoPath;

        private const string _origin = "https://github.com/NuKeeperDotNet/NuKeeper.git";

        [OneTimeSetUp]
        public void Setup()
        {
            _logger = new ConfigurableLogger();
            _pathTogit = TestDirectoryHelper.DiscoverPathToGit();
            if (_pathTogit == null)
            {
                Assert.Ignore("no git implementation found!");
            }

            // create a local repo to test against
            var folder = TestDirectoryHelper.GenerateRandomSubFolder(TestContext.CurrentContext.WorkDirectory);
            var gitDriver = new GitCmdDriver(_pathTogit, _logger, new Folder(_logger, folder), new Abstractions.Git.GitUsernamePasswordCredentials());
            Assert.DoesNotThrowAsync(() => gitDriver.Clone(new Uri(_origin)));
            _repoPath = folder.FullName;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TestDirectoryHelper.DeleteDirectory(_repoPath);
        }

        [Test]
        public async Task ShouldDiscoverLocalRepo()
        {
            var gitDiscoveryDriver = new GitCmdDiscoveryDriver(_pathTogit, _logger);
            var repo = await gitDiscoveryDriver.DiscoverRepo(new Uri(_repoPath));
            Assert.AreEqual(_origin, repo.AbsoluteUri);
        }

        [Test]
        public async Task ShouldGetRemotes()
        {
            var gitDiscoveryDriver = new GitCmdDiscoveryDriver(_pathTogit, _logger);
            var classicgitDiscoveryDriver = new LibGit2SharpDiscoveryDriver(_logger);
            var remotes = await gitDiscoveryDriver.GetRemotes(new Uri(_repoPath));
            var classicRemotes = await classicgitDiscoveryDriver.GetRemotes(new Uri(_repoPath));

            // Lib2Sharp and GitCmd should have the same results
        }
    }
}
