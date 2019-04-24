using System;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Gitea;
using NUnit.Framework;

namespace NuKeeper.Gitea.Tests
{
    [TestFixture]
    public class GiteaSettingsReaderTests
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

        /// <summary>
        /// Test for #739
        /// </summary>
        [Test]
        public void CanRead_NoException_OnBadUri()
        {
            Assert.DoesNotThrow(() => _giteaSettingsReader.CanRead(new Uri("https://try.gitea.io/")));
        }

        [Test]
        public void UpdatesAuthenticationTokenFromTheEnvironment()
        {
            _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_gitea_token").Returns("envToken");

            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };

            _giteaSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.AreEqual("envToken", settings.Token);
        }

        [Test]
        public void AssumesItCanReadGiteaUrls()
        {
            var canRead = _giteaSettingsReader.CanRead(new Uri("https://try.gitea.io/SharpSteff/NuKeeper-TestFork"));

            Assert.AreEqual(true, canRead);
        }

        [Test]
        public void AssumesItCanNotReadGitHubUrls()
        {
            var canRead = _giteaSettingsReader.CanRead(new Uri("https://github.com/SharpSteff/NuKeeper-TestFork"));

            Assert.AreNotEqual(true, canRead);
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
            Assert.AreEqual("NuKeeper-TestFork", repositorySettings.RepositoryName);
            Assert.AreEqual(targetBranch, repositorySettings.RemoteInfo?.BranchName);
        }
    }
}
