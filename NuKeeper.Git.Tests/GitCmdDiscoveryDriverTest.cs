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
        private DirectoryInfo _repo;

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
            var folder = TestDirectoryHelper.UniqueTemporaryFolder();
            var gitDriver = new GitCmdDriver(_pathTogit, _logger, new Folder(_logger, folder), new Abstractions.Git.GitUsernamePasswordCredentials());
            Assert.DoesNotThrowAsync(() => gitDriver.Clone(new Uri(_origin)));
            _repo = folder;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            TestDirectoryHelper.DeleteDirectory(_repo);
        }

        [Test]
        public async Task ShouldDiscoverLocalRepo()
        {
            var gitDiscoveryDriver = new GitCmdDiscoveryDriver(_pathTogit, _logger);
            var repo = await gitDiscoveryDriver.DiscoverRepo(new Uri(_repo.FullName));
            Assert.AreEqual(_origin, repo.AbsoluteUri);
        }

        [Test]
        public async Task ShouldGetRemotes()
        {
            var gitDiscoveryDriver = new GitCmdDiscoveryDriver(_pathTogit, _logger);
            var classicGitDiscoveryDriver = new LibGit2SharpDiscoveryDriver(_logger);
            var remotes = await gitDiscoveryDriver.GetRemotes(new Uri(_repo.FullName));
            var classicRemotes = await classicGitDiscoveryDriver.GetRemotes(new Uri(_repo.FullName));

            var remotesArray = remotes?.ToArray();
            var classicRemotesArray = classicRemotes?.ToArray();

            Assert.IsNotNull(remotesArray, "GitCmdDiscoveryDriver returned null for GetRemotes");
            Assert.IsNotNull(classicRemotesArray, "LibGit2SharpDiscoveryDriver returned null for GetRemotes");

            Assert.AreEqual(remotesArray?.Count(), classicRemotesArray?.Count(), "Lib2Sharp and GitCmd should have the same number of results");

            for(var count=0; count< classicRemotesArray.Count(); count++)
            {
                var classicRemote = classicRemotesArray[count];
                var remote = remotesArray.Where(r => r.Name.Equals(classicRemote.Name, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                Assert.NotNull(classicRemote, $"GitCmd does not find remote {remote.Name}");
                Assert.AreEqual(classicRemote.Url, remote.Url, $"GitCmd does return the same url: {remote.Url}");
            }
        }
    }
}
