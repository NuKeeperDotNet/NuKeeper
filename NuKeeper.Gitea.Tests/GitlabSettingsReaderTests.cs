using System;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Gitea;
using NUnit.Framework;

namespace NuKeeper.Gitea.Tests
{
    [TestFixture]
    public class GitlabSettingsReaderTests
    {
        private GiteaSettingsReader _giteaSettingsReader;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariablesProvider = Substitute.For<IEnvironmentVariablesProvider>();
            _giteaSettingsReader = new GiteaSettingsReader(_environmentVariablesProvider);
        }

        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = _giteaSettingsReader.Platform;

            Assert.AreEqual(Platform.Gitea, platform);
        }

        [Test]
        public void UpdatesAuthenticationTokenFromTheEnvironment()
        {
            _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_gitlab_token").Returns("envToken");

            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };

            _giteaSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.AreEqual(settings.Token, "envToken");
        }

        [Test]
        public void AssumesItCanReadGitLabUrls()
        {
            var canRead = _giteaSettingsReader.CanRead(new Uri("https://try.gitea.io/SharpSteff/NuKeeper-TestFork"));

            Assert.AreEqual(true, canRead);
        }

        [TestCase(null)]
        [TestCase("master")]
        public void GetsCorrectSettingsFromTheUrl(string targetBranch)
        {
            var repositoryUri = new Uri("https://try.gitea.io/SharpSteff/NuKeeper-TestFork");
            var repositorySettings = _giteaSettingsReader.RepositorySettings(repositoryUri, targetBranch);

            Assert.IsNotNull(repositorySettings);
            Assert.AreEqual(new Uri("https://try.gitea.io/api/v1/"), repositorySettings.ApiUri);
            Assert.AreEqual(repositoryUri, repositorySettings.RepositoryUri);
            Assert.AreEqual("SharpSteff", repositorySettings.RepositoryOwner);
            Assert.AreEqual("NuKeeperTestFork", repositorySettings.RepositoryName);
            Assert.AreEqual(targetBranch, repositorySettings.RemoteInfo?.BranchName);
        }
    }
}
