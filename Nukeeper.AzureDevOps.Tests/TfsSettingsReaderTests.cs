using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.AzureDevOps;
using NuKeeper.Tests;
using NUnit.Framework;

namespace Nukeeper.AzureDevOps.Tests
{
    [TestFixture]
    public class TfsSettingsReaderTests
    {
        private ISettingsReader _azureSettingsReader;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariablesProvider = Substitute.For<IEnvironmentVariablesProvider>();
            _azureSettingsReader = new TfsSettingsReader(new MockedGitDiscoveryDriver(), _environmentVariablesProvider);
        }

        [TestCase("https://tfs/")]
        [TestCase("https://internalserver/tfs/")]
        public async Task ReturnsTrueIfCanRead(string value)
        {
            var canRead = await _azureSettingsReader.CanRead(new Uri(value));
            Assert.IsTrue(canRead);
        }

        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = _azureSettingsReader.Platform;
            Assert.IsNotNull(platform);
            Assert.AreEqual(platform, Platform.AzureDevOps);
        }

        [Test]
        public void UpdateSettings_UpdatesSettings()
        {
            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
                BaseApiUrl = new Uri("https://internalserver/tfs/")
            };
            _azureSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.BaseApiUrl, "https://internalserver/tfs/");
            Assert.AreEqual(settings.Token, "accessToken");
            Assert.AreEqual(settings.ForkMode, ForkMode.SingleRepositoryOnly);
        }

        [Test]
        public void AuthSettings_GetsCorrectSettingsFromEnvironment()
        {
            _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_azure_devops_token").Returns("envToken");

            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };

            _azureSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.AreEqual(settings.Token, "envToken");
        }

        [TestCase(null)]
        [TestCase("htps://dev.azure.com")]
        public async Task InvalidUrlReturnsNull(string value)
        {
            var uriToTest = value == null ? null : new Uri(value);
            var canRead = await _azureSettingsReader.CanRead(uriToTest);

            Assert.IsFalse(canRead);
        }

        [Test]
        public async Task RepositorySettings_GetsCorrectSettings()
        {
            var settings = await _azureSettingsReader.RepositorySettings(new Uri("https://internalserver/tfs/project/_git/reponame"), true);

            Assert.IsNotNull(settings);
            Assert.AreEqual("https://internalserver/tfs", settings.ApiUri.ToString());
            Assert.AreEqual("https://user:--PasswordToReplace--@internalserver/tfs/project/_git/reponame/", settings.RepositoryUri.ToString());
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "project");
            Assert.AreEqual(settings.SetAutoComplete, true);
        }

        [Test]
        public async Task RepositorySettings_ReturnsNull()
        {
            var settings = await _azureSettingsReader.RepositorySettings(null, true);
            Assert.IsNull(settings);
        }

        [Test]
        public void RepositorySettings_InvalidFormat()
        {
            Assert.ThrowsAsync<NuKeeperException>(() =>
                _azureSettingsReader.RepositorySettings(
                    new Uri("https://org.visualstudio.com/project/isnot_git/reponame/"), true));
        }

        [Test]
        public async Task RepositorySettings_HandlesSpaces()
        {
            var settings = await _azureSettingsReader.RepositorySettings(new Uri("https://internalserver/tfs/project%20name/_git/repo%20name"), true);

            Assert.IsNotNull(settings);
            Assert.AreEqual("https://internalserver/tfs", settings.ApiUri.ToString());
            Assert.AreEqual("https://user:--PasswordToReplace--@internalserver/tfs/project%20name/_git/repo%20name/", settings.RepositoryUri.AbsoluteUri);
            Assert.AreEqual("repo name", settings.RepositoryName);
            Assert.AreEqual("project name", settings.RepositoryOwner);
        }

    }
}
