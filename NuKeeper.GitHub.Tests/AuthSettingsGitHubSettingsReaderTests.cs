using System;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class AuthSettingsGitHubSettingsReaderTests
    {
#pragma warning disable CA1054 // Uri parameters should not be strings
#pragma warning disable CA1051 // Do not declare visible instance fields

        private static GitHubSettingsReader GitHubSettingsReader => new GitHubSettingsReader();

        [Test]
        public void AuthSettings_GetsCorrectSettings()
        {
            var settings = GitHubSettingsReader.AuthSettings(new Uri("https://github.custom.com/"), "accessToken");

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://github.custom.com/");
            Assert.AreEqual(settings.Token, "accessToken");
        }

        [Test]
        public void AuthSettings_GetsCorrectSettingsFromEnvironment()
        {
            Environment.SetEnvironmentVariable("NuKeeper_github_token","envToken");
            var settings = GitHubSettingsReader.AuthSettings(new Uri("https://github.custom.com/"), "accessToken");
            Environment.SetEnvironmentVariable("NuKeeper_github_token",null);

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://github.custom.com/");
            Assert.AreEqual(settings.Token, "envToken");

        }

        [Test]
        public void GetsCorrectSettingWithMissingSlash()
        {
            var settings = GitHubSettingsReader.AuthSettings(new Uri("https://api.github.com"), "accessToken");

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://api.github.com/");
            Assert.AreEqual(settings.Token, "accessToken");
        }

        [DatapointSource]
        public Uri[] Values = {
            null,
            new Uri("htps://missingt.com"),
        };
        [Theory]
        public void InvalidUrlReturnsNull(Uri uri)
        {
            var settings = GitHubSettingsReader.AuthSettings(uri, "accessToken");

            Assert.IsNull(settings);
        }
    }

}
