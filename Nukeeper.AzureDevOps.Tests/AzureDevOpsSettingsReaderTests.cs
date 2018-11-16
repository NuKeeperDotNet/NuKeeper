using System;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.AzureDevOps;
using NUnit.Framework;

namespace Nukeeper.AzureDevOps.Tests
{
    [TestFixture]
    public class AzureDevOpsSettingsReaderTests
    {
        private static ISettingsReader AzureSettingsReader => new AzureDevOpsSettingsReader(new MockedGitDiscoveryDriver());

        [Test]
        public void ReturnsTrueIfCanRead()
        {
            var canRead = AzureSettingsReader.CanRead(new Uri("https://dev.azure.com/org"));
            Assert.IsTrue(canRead);
        }

        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = AzureSettingsReader.Platform;
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
            AzureSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.BaseApiUrl, "https://dev.azure.com/");
            Assert.AreEqual(settings.Token, "accessToken");
            Assert.AreEqual(settings.ForkMode, ForkMode.SingleRepositoryOnly);
        }

        [Test]
        public void AuthSettings_GetsCorrectSettingsFromEnvironment()
        {
            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };
            Environment.SetEnvironmentVariable("NuKeeper_azure_devops_token", "envToken");
            AzureSettingsReader.UpdateCollaborationPlatformSettings(settings);
            Environment.SetEnvironmentVariable("NuKeeper_azure_devops_token", null);

            Assert.AreEqual(settings.Token, "envToken");
        }

        [TestCase(null)]
        [TestCase("htps://org.visualstudio.com")]
        public void InvalidUrlReturnsNull(string value)
        {
            var uriToTest = value == null ? null : new Uri(value);
            var canRead = AzureSettingsReader.CanRead(uriToTest);

            Assert.IsFalse(canRead);
        }

        [Test]
        public void RepositorySettings_GetsCorrectSettings()
        {
            var settings = AzureSettingsReader.RepositorySettings(new Uri("https://dev.azure.com/org/project/_git/reponame"));

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiUri, "https://dev.azure.com/org/");
            Assert.AreEqual(settings.RepositoryUri, "https://dev.azure.com/org/project/_git/reponame");
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "project");
        }

        [Test]
        public void RepositorySettings_ReturnsNull()
        {
            var settings = AzureSettingsReader.RepositorySettings(null);
            Assert.IsNull(settings);
        }

        [Test]
        public void RepositorySettings_PathTooLong()
        {
            var settings = AzureSettingsReader.RepositorySettings(new Uri("https://dev.azure.com/org/project/_git/reponame/thisShouldNotBeHere/"));
            Assert.IsNull(settings);
        }
    }
}
