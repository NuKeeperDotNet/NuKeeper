using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NUnit.Framework;

namespace NuKeeper.Gitlab.Tests
{
    [TestFixture]
    public class GitlabSettingsReaderTests
    {
        private GitlabSettingsReader _gitlabSettingsReader;
        private IEnvironmentVariablesProvider _environmentVariablesProvider;

        [SetUp]
        public void Setup()
        {
            _environmentVariablesProvider = Substitute.For<IEnvironmentVariablesProvider>();

            _gitlabSettingsReader = new GitlabSettingsReader(_environmentVariablesProvider);
        }

        [Test]
        public void ReturnsCorrectPlatform()
        {
            var platform = _gitlabSettingsReader.Platform;

            Assert.AreEqual(Platform.GitLab, platform);
        }

        [Test]
        public void UpdatesAuthenticationTokenFromTheEnvironment()
        {
            _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_gitlab_token").Returns("envToken");

            var settings = new CollaborationPlatformSettings
            {
                Token = "accessToken",
            };

            _gitlabSettingsReader.UpdateCollaborationPlatformSettings(settings);

            Assert.AreEqual(settings.Token, "envToken");
        }

        [Test]
        public async Task AssumesItCanReadGitLabUrls()
        {
            var canRead = await _gitlabSettingsReader.CanRead(new Uri("https://gitlab.com/user/projectname.git"));

            Assert.AreEqual(true, canRead);
        }

        [TestCase(null)]
        [TestCase("master")]
        public async Task GetsCorrectSettingsFromTheUrl(string targetBranch)
        {
            var repositoryUri = new Uri("https://gitlab.com/user/projectname.git");
            var repositorySettings = await _gitlabSettingsReader.RepositorySettings(repositoryUri, true, targetBranch);

            Assert.IsNotNull(repositorySettings);
            Assert.AreEqual(new Uri("https://gitlab.com/api/v4/"), repositorySettings.ApiUri);
            Assert.AreEqual(repositoryUri, repositorySettings.RepositoryUri);
            Assert.AreEqual("user", repositorySettings.RepositoryOwner);
            Assert.AreEqual("projectname", repositorySettings.RepositoryName);
            Assert.AreEqual(targetBranch, repositorySettings.RemoteInfo?.BranchName);
            Assert.AreEqual(false, repositorySettings.SetAutoMerge);
        }
    }
}
