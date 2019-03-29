using System;
using NuKeeper.Abstractions.CollaborationPlatform;
using NUnit.Framework;

namespace NuKeeper.Gitlab.Tests
{
    [TestFixture]
    public class GitlabSettingsReaderTests
    {
        private GitlabSettingsReader _gitlabSettingsReader;


        [SetUp]
        public void Setup()
        {
            _gitlabSettingsReader = new GitlabSettingsReader();
        }


        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = _gitlabSettingsReader.Platform;

            Assert.AreEqual(Platform.GitLab, platform);
        }


        [Test]
        public void AssumesItCanReadGitLabUrls()
        {
            var canRead = _gitlabSettingsReader.CanRead(new Uri("https://gitlab.com/user/projectname.git"));

            Assert.AreEqual(true, canRead);
        }

        [Test]
        public void GetsCorrectSettingsFromTheUrl()
        {
            var repositoryUri = new Uri("https://gitlab.com/user/projectname.git");
            var repositorySettings = _gitlabSettingsReader.RepositorySettings(repositoryUri, "master");

            Assert.IsNotNull(repositorySettings);
            Assert.AreEqual(new Uri("https://gitlab.com/api/v4/"), repositorySettings.ApiUri);
            Assert.AreEqual(repositoryUri, repositorySettings.RepositoryUri);
            Assert.AreEqual("user", repositorySettings.RepositoryOwner);
            Assert.AreEqual("projectname", repositorySettings.RepositoryName);
            Assert.AreEqual("master", repositorySettings.TargetBranch);
        }
    }
}
