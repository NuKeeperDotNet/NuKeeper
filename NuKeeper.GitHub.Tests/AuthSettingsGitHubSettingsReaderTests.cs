using System;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class AuthSettingsGitHubSettingsReaderTests
    {
#pragma warning disable CA1054 // Uri parameters should not be strings
#pragma warning disable CA1051 // Do not declare visible instance fields

        private static GitHubSettingsReader GitHubSettingsReader
        {
            get
            {
                var settingsCache = Substitute.For<IFileSettingsCache>();
                settingsCache.GetSettings().Returns(FileSettings.Empty());
                return new GitHubSettingsReader(settingsCache);
            }
        }

        [Test]
        public void AuthSettings_GetsCorrectSettings()
        {
            var settings = GitHubSettingsReader.AuthSettings("https://github.custom.com/", "accessToken");

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://github.custom.com/");
            Assert.AreEqual(settings.Token, "accessToken");
        }

        [Test]
        public void AuthSettings_GetsCorrectSettingsFromEnvironment()
        {
            Environment.SetEnvironmentVariable("NuKeeper_github_token","envToken");
            var settings = GitHubSettingsReader.AuthSettings("https://github.custom.com/", "accessToken");
            Environment.SetEnvironmentVariable("NuKeeper_github_token",null);

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://github.custom.com/");
            Assert.AreEqual(settings.Token, "envToken");

        }

        [Test]
        public void GetsCorrectSettingsFromFile()
        {
            var settingsCache = Substitute.For<IFileSettingsCache>();
            settingsCache.GetSettings().Returns(new FileSettings { Api = "https://github.fromfile.com/" });
            var gitHubSettingsReader = new GitHubSettingsReader(settingsCache);

            var settings = gitHubSettingsReader.AuthSettings(null, "accessToken");

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://github.fromfile.com/");
            Assert.AreEqual(settings.Token, "accessToken");
        }

        [Test]
        public void GetsCorrectSettingFromFallback()
        {
            var settings = GitHubSettingsReader.AuthSettings(null, "accessToken");

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://api.github.com/");
            Assert.AreEqual(settings.Token, "accessToken");
        }

        [Test]
        public void GetsCorrectSettingWithMissingSlash()
        {
            var settings = GitHubSettingsReader.AuthSettings("https://api.github.com", "accessToken");

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.ApiBase, "https://api.github.com/");
            Assert.AreEqual(settings.Token, "accessToken");
        }

        [DatapointSource]
        public string[] AuthSettingBadUrlValues = {
            "https://github.com/owner/",
            "htps://github.com/"
        };
        [Theory]
        public void InvalidUrlReturnsNull(string uri)
        {
            var settings = GitHubSettingsReader.AuthSettings(uri, "accessToken");

            Assert.IsNull(settings);
        }
    }

}
