using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Creators;
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

            var count = await engine.Run(new SettingsContainer
            {
                GithubAuthSettings = MakeGitHubAuthSettings(),
                UserSettings = new UserSettings
                {
                    MaxRepositoriesChanged = 1
                }
            });

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
            var githubCreator = Substitute.For<ICreate<IGitHub>>();
            var repositoryDiscoveryCreator = Substitute.For<ICreate<IGitHubRepositoryDiscovery>>();
            var repoEngineCreator = Substitute.For<ICreate<IGitHubRepositoryEngine>>();

            github.GetCurrentUser().Returns(
                RepositoryBuilder.MakeUser("http://test.user.com"));

            githubCreator.Create(null).ReturnsForAnyArgs(github);

            repoDiscovery.GetRepositories()
                .Returns(repos);

            repositoryDiscoveryCreator.Create(null).ReturnsForAnyArgs(repoDiscovery);

            repoEngine.Run(null, null, null).ReturnsForAnyArgs(repoEngineResult);

            repoEngineCreator.Create(null).ReturnsForAnyArgs(repoEngine);

            var engine = new GitHubEngine(githubCreator, repositoryDiscoveryCreator, repoEngineCreator,
                folders, Substitute.For<INuKeeperLogger>());
            return engine;
        }

        private static SettingsContainer MakeSettings()
        {
            return new SettingsContainer
            {
                GithubAuthSettings = MakeGitHubAuthSettings(),
                UserSettings = MakeUserSettings()
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
