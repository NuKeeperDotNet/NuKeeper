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
    public class AzureDevOpsSettingsReaderTests
    {
        private ISettingsReader _azureSettingsReader;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariablesProvider = Substitute.For<IEnvironmentVariablesProvider>();
            _azureSettingsReader = new AzureDevOpsSettingsReader(new MockedGitDiscoveryDriver(), _environmentVariablesProvider);
        }

        [Test]
        public async Task ReturnsTrueIfCanRead()
        {
            var canRead = await _azureSettingsReader.CanRead(new Uri("https://dev.azure.com/org"));
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
                BaseApiUrl = new Uri("https://dev.azure.com/")
            };
            _azureSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.BaseApiUrl, "https://dev.azure.com/");
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
        [TestCase("htps://org.visualstudio.com")]
        public async Task InvalidUrlReturnsNull(string value)
        {
            var uriToTest = value == null ? null : new Uri(value);
            var canRead = await _azureSettingsReader.CanRead(uriToTest);

            Assert.IsFalse(canRead);
        }

        [Test]
        public async Task RepositorySettings_GetsCorrectSettingsOrganisation()
        {
            var settings = await _azureSettingsReader.RepositorySettings(new Uri("https://dev.azure.com/org/project/_git/reponame"));

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiUri, "https://dev.azure.com/org/");
            Assert.AreEqual(settings.RepositoryUri, "https://dev.azure.com/org/project/_git/reponame");
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "project");
        }

        [Test]
        public async Task RepositorySettings_GetsCorrectSettingsPrivate()
        {
            var settings = await _azureSettingsReader.RepositorySettings(new Uri("https://dev.azure.com/owner/_git/reponame"));

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiUri, "https://dev.azure.com/");
            Assert.AreEqual(settings.RepositoryUri, "https://dev.azure.com/owner/_git/reponame");
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "owner");
        }

        [Test]
        public async Task RepositorySettings_ReturnsNull()
        {
            var settings = await _azureSettingsReader.RepositorySettings(null);
            Assert.IsNull(settings);
        }

        [Test]
        public void RepositorySettings_PathTooLong()
        {
            Assert.ThrowsAsync<NuKeeperException>(() => _azureSettingsReader.RepositorySettings(new Uri("https://dev.azure.com/org/project/_git/reponame/thisShouldNotBeHere/")));

        }

        [Test]
        public async Task RepositorySettings_HandlesSpacesInRepo()
        {
            var settings = await _azureSettingsReader.RepositorySettings(new Uri("https://dev.azure.com/org/project%20name/_git/repo%20name"));

            Assert.IsNotNull(settings);
            Assert.AreEqual("https://dev.azure.com/org/", settings.ApiUri.ToString());
            Assert.AreEqual("https://dev.azure.com/org/project%20name/_git/repo%20name", settings.RepositoryUri.AbsoluteUri);
            Assert.AreEqual("repo name", settings.RepositoryName);
            Assert.AreEqual("project name", settings.RepositoryOwner);
        }

    }
}
