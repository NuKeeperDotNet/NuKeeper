using System;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class AuthSettingsGitHubSettingsReaderTests
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

#pragma warning disable CA1051 // Do not declare visible instance fields
        [DatapointSource] public Uri[] Values =
        {
            null,
            new Uri("htps://missingt.com"),
        };
        [Theory]
        public void InvalidUrlReturnsNull(Uri uri)
        {
            var canRead = GitHubSettingsReader.CanRead(uri);

            Assert.IsFalse(canRead);
        }
    }
}
