using System;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class RepositorySettingsGitHubSettingsReaderTests
    {
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
        public void RepositorySettings_GetsCorrectSettings()
        {
            var settings = GitHubSettingsReader.RepositorySettings(new Uri("https://github.com/owner/reponame.git"));

            Assert.IsNotNull(settings);
            Assert.AreEqual(settings.Uri, "https://github.com/owner/reponame.git");
            Assert.AreEqual(settings.RepositoryName, "reponame");
            Assert.AreEqual(settings.RepositoryOwner, "owner");
        }

        [DatapointSource]
        public Uri[] Values = {
            null,
            new Uri("https://github.com/owner/badpart/reponame.git"),
            new Uri("https://github.com/reponame.git")
        };
        [Theory]
        public void RepositorySettings_InvalidUrlReturnsNull(Uri uri)
        {
            var settings = GitHubSettingsReader.RepositorySettings(uri);

            Assert.IsNull(settings);
        }
    }
}
