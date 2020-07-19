using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Tests;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class GitHubSettingsReaderTests
    {
        private GitHubSettingsReader _gitHubSettingsReader;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariablesProvider = Substitute.For<IEnvironmentVariablesProvider>();
            _gitHubSettingsReader = new GitHubSettingsReader(new MockedGitDiscoveryDriver(), _environmentVariablesProvider);
        }

        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = _gitHubSettingsReader.Platform;
            Assert.IsNotNull(platform);
            Assert.AreEqual(platform, Platform.GitHub);
        }

        [Test]
        public void UpdateSettings_UpdatesSettings()
        {
            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
                BaseApiUrl = new Uri("https://github.custom.com/")
            };
            _gitHubSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.IsNotNull(settings);
            Assert.AreEqual(new Uri("https://github.custom.com/"), settings.BaseApiUrl);
            Assert.AreEqual("accessToken", settings.Token);
            Assert.AreEqual(ForkMode.PreferFork, settings.ForkMode);
        }

        [Test]
        public void AuthSettings_GetsCorrectSettingsFromEnvironment()
        {
            _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_github_token").Returns("envToken");

            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };

            _gitHubSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.AreEqual("envToken", settings.Token);
        }

        [TestCase(null)]
        [TestCase("htps://missingt.com")]
        public async Task InvalidUrlReturnsNull(string value)
        {
            var testUri = value == null ? null : new Uri(value);
            var canRead = await _gitHubSettingsReader.CanRead(testUri);

            Assert.IsFalse(canRead);
        }

        [Test]
        public async Task RepositorySettings_GetsCorrectSettings()
        {
            var settings = await _gitHubSettingsReader.RepositorySettings(new Uri("https://github.com/owner/reponame.git"), true);

            Assert.IsNotNull(settings);
            Assert.AreEqual(new Uri("https://github.com/owner/reponame.git"), settings.RepositoryUri);
            Assert.AreEqual("reponame", settings.RepositoryName);
            Assert.AreEqual("owner", settings.RepositoryOwner);
            Assert.AreEqual(false, settings.SetAutoComplete);
        }

        [Test]
        public async Task RepositorySettings_GetsCorrectSettingsWithTargetBranch()
        {
            var settings =
                await _gitHubSettingsReader.RepositorySettings(new Uri("https://github.com/owner/reponame.git"), true,"Feature1");

            Assert.IsNotNull(settings);
            Assert.AreEqual(new Uri("https://github.com/owner/reponame.git"), settings.RepositoryUri);
            Assert.AreEqual("reponame", settings.RepositoryName);
            Assert.AreEqual("owner", settings.RepositoryOwner);
            Assert.NotNull(settings.RemoteInfo);
            Assert.AreEqual("Feature1", settings.RemoteInfo.BranchName);
            Assert.AreEqual(false, settings.SetAutoComplete);
        }

        [TestCase(null)]
        [TestCase("https://github.com/owner/badpart/reponame.git")]
        public void RepositorySettings_InvalidUrlReturnsNull(string value)
        {
            var testUri = value == null ? null : new Uri(value);
            Assert.ThrowsAsync<NuKeeperException>(() => _gitHubSettingsReader.RepositorySettings(testUri, true));
        }
    }
}
