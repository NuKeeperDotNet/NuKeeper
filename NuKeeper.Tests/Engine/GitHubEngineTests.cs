using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GitHubEngineTests
    {
        [Test]
        public async Task SuccessCaseWithNoRepos()
        {
            var engine = MakeGithubEngine(
                new List<RepositorySettings>());

            var count = await engine.Run(MakeSettings());

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task SuccessCaseWithOneRepo()
        {
            var oneRepo = new List<RepositorySettings>
            {
                new RepositorySettings()
            };
            var engine = MakeGithubEngine(oneRepo);

            var count = await engine.Run(MakeSettings());

            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public async Task SuccessCaseWithTwoRepos()
        {
            var repos = new List<RepositorySettings>
            {
                new RepositorySettings(),
                new RepositorySettings()
            };
            var engine = MakeGithubEngine(repos);

            var count = await engine.Run(MakeSettings());

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task SuccessCaseWithTwoReposAndTwoPrsPerRepo()
        {
            var repos = new List<RepositorySettings>
            {
                new RepositorySettings(),
                new RepositorySettings()
            };
            var engine = MakeGithubEngine(2, repos);

            var count = await engine.Run(MakeSettings());

            Assert.That(count, Is.EqualTo(2));
        }


        [Test]
        public async Task CountIsNotIncrementedWhenRepoEngineFails()
        {
            var repos = new List<RepositorySettings>
            {
                new RepositorySettings(),
                new RepositorySettings(),
                new RepositorySettings()
            };
            var engine = MakeGithubEngine(0, repos);

            var count = await engine.Run(MakeSettings());

            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public async Task WhenThereIsAMaxNumberOfRepos()
        {
            var repos = new List<RepositorySettings>
            {
                new RepositorySettings(),
                new RepositorySettings(),
                new RepositorySettings()
            };

            var engine = MakeGithubEngine(1, repos);

            var settings = new SettingsContainer
            {
                GithubAuthSettings = MakeGitHubAuthSettings(),
                UserSettings = new UserSettings
                {
                    MaxRepositoriesChanged = 1
                },
                SourceControlServerSettings = MakseServerSettings()
            };

            var count = await engine.Run(settings);

            Assert.That(count, Is.EqualTo(1));
        }

        private static GitHubEngine MakeGithubEngine(
            List<RepositorySettings> repos)
        {
            return MakeGithubEngine(1, repos);
        }

        private static GitHubEngine MakeGithubEngine(
            int repoEngineResult,
            List<RepositorySettings> repos)
        {
            var github = Substitute.For<IGitHub>();
            var repoDiscovery = Substitute.For<IGitHubRepositoryDiscovery>();
            var repoEngine = Substitute.For<IGitHubRepositoryEngine>();
            var folders = Substitute.For<IFolderFactory>();

            github.GetCurrentUser().Returns(
                RepositoryBuilder.MakeUser("http://test.user.com"));

            repoDiscovery.GetRepositories(Arg.Any<IGitHub>(), Arg.Any<SourceControlServerSettings>())
                .Returns(repos);

            repoEngine.Run(null, null, null, null).ReturnsForAnyArgs(repoEngineResult);

            var engine = new GitHubEngine(github, repoDiscovery, repoEngine,
                folders, Substitute.For<INuKeeperLogger>());
            return engine;
        }

        private static SettingsContainer MakeSettings()
        {
            return new SettingsContainer
            {
                GithubAuthSettings = MakeGitHubAuthSettings(),
                UserSettings = MakeUserSettings(),
                SourceControlServerSettings = MakseServerSettings()
            };
        }

        private static SourceControlServerSettings MakseServerSettings()
        {
            return new SourceControlServerSettings
            {
                Scope = ServerScope.Repository
            };
        }

        private static UserSettings MakeUserSettings()
        {
            return new UserSettings { MaxRepositoriesChanged = int.MaxValue };
        }

        private static GithubAuthSettings MakeGitHubAuthSettings()
        {
            return new GithubAuthSettings(new Uri("http://foo.com"), "token123");
        }
    }
}
