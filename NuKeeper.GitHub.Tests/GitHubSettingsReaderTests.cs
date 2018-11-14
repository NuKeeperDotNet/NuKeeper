using System;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class GitHubSettingsReaderTests
    {
        private static GitHubSettingsReader GitHubSettingsReader => new GitHubSettingsReader();

        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = GitHubSettingsReader.Platform;
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
            GitHubSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.BaseApiUrl, "https://github.custom.com/");
            Assert.AreEqual(settings.Token, "accessToken");
            Assert.AreEqual(settings.ForkMode, ForkMode.PreferFork);
        }

        [Test]
        public void AuthSettings_GetsCorrectSettingsFromEnvironment()
        {
            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };
            Environment.SetEnvironmentVariable("NuKeeper_github_token", "envToken");
            GitHubSettingsReader.UpdateCollaborationPlatformSettings(settings);
            Environment.SetEnvironmentVariable("NuKeeper_github_token", null);

            Assert.AreEqual(settings.Token, "envToken");
        }

        [TestCase(null)]
        [TestCase("htps://missingt.com")]
        public void InvalidUrlReturnsNull(string value)
        {
            var testUri = value == null ? null : new Uri(value);
            var canRead = GitHubSettingsReader.CanRead(testUri);

            Assert.IsFalse(canRead);
        }

        [Test]
        public void RepositorySettings_GetsCorrectSettings()
        {
            var settings = GitHubSettingsReader.RepositorySettings(new Uri("https://github.com/owner/reponame.git"));

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.RepositoryUri, "https://github.com/owner/reponame.git");
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "owner");
        }

        [TestCase(null)]
        [TestCase("https://github.com/owner/badpart/reponame.git")]
        [TestCase("https://github.com/reponame.git")]
        public void RepositorySettings_InvalidUrlReturnsNull(string value)
        {
            var testUri = value == null ? null : new Uri(value);
            var settings = GitHubSettingsReader.RepositorySettings(testUri);

            Assert.IsNull(settings);
        }
    }
}
