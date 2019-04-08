using System;
using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.AzureDevOps;
using NUnit.Framework;

namespace Nukeeper.AzureDevOps.Tests
{
    [TestFixture]
    public class VstsSettingsReaderTests
    {
        private ISettingsReader _azureSettingsReader;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariablesProvider = Substitute.For<IEnvironmentVariablesProvider>();
            _azureSettingsReader = new VstsSettingsReader(new MockedGitDiscoveryDriver(), _environmentVariablesProvider);
        }

        [Test]
        public void ReturnsTrueIfCanRead()
        {
            var canRead = _azureSettingsReader.CanRead(new Uri("https://org.visualstudio.com"));
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
        [TestCase("htps://dev.azure.com")]
        public void InvalidUrlReturnsNull(string value)
        {
            var uriToTest = value == null ? null : new Uri(value);
            var canRead = _azureSettingsReader.CanRead(uriToTest);

            Assert.IsFalse(canRead);
        }

        [Test]
        public void RepositorySettings_GetsCorrectSettings()
        {
            var settings = _azureSettingsReader.RepositorySettings(new Uri("https://org.visualstudio.com/project/_git/reponame"));

            Assert.IsNotNull(settings);
            Assert.AreEqual("https://org.visualstudio.com/", settings.ApiUri.ToString());
            Assert.AreEqual("https://org.visualstudio.com/project/_git/reponame/", settings.RepositoryUri.ToString());
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "project");
        }

        [Test]
        public void RepositorySettings_ReturnsNull()
        {
            var settings = _azureSettingsReader.RepositorySettings(null);
            Assert.IsNull(settings);
        }

        [Test]
        public void RepositorySettings_InvalidFormat()
        {
            Assert.Throws<NuKeeperException>(() =>
                _azureSettingsReader.RepositorySettings(
                    new Uri("https://org.visualstudio.com/project/_git/reponame/thisShouldNotBeHere/")));
        }
    }
}
