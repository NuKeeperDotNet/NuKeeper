using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibGit2Sharp;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.Inspection.Files;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GithubEngineTests
    {
        [Test]
        public async Task SuccessCaseWithNoRepos()
        {
            var engine = MakeGithubEngine(
                new List<RepositorySettings>());

            var count = await engine.Run();

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

            var count = await engine.Run();

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

            var count = await engine.Run();

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

            var count = await engine.Run();

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

            var count = await engine.Run();

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

            var settings = new UserSettings
            {
                MaxRepositoriesChanged = 1
            };

            var engine = MakeGithubEngine(1, settings, repos);

            var count = await engine.Run();

            Assert.That(count, Is.EqualTo(1));
        }

        private static GithubEngine MakeGithubEngine(
            List<RepositorySettings> repos)
        {
            return MakeGithubEngine(1,
                new UserSettings { MaxRepositoriesChanged = int.MaxValue },
                repos);
        }

        private static GithubEngine MakeGithubEngine(
            int repoEngineResult,
            List<RepositorySettings> repos)
        {
            return MakeGithubEngine(repoEngineResult,
                new UserSettings { MaxRepositoriesChanged = int.MaxValue },
                repos);
        }

        private static GithubEngine MakeGithubEngine(
            int repoEngineResult,
            UserSettings userSettings,
            List<RepositorySettings> repos)
        {
            var github = Substitute.For<IGithub>();
            var repoDiscovery = Substitute.For<IGithubRepositoryDiscovery>();
            var repoEngine = Substitute.For<IGithubRepositoryEngine>();
            var folders = Substitute.For<IFolderFactory>();

            github.GetCurrentUser().Returns(
                RepositoryBuilder.MakeUser("http://test.user.com"));

            repoDiscovery.GetRepositories()
                .Returns(repos);

            repoEngine.Run(
                    Arg.Any<RepositorySettings>(),
                    Arg.Any<UsernamePasswordCredentials>(),
                    Arg.Any<Identity>())
                .Returns(repoEngineResult);

            var githubAuthSettings = new GithubAuthSettings(new Uri("http://foo.com"), "token123");

            var engine = new GithubEngine(github, repoDiscovery, repoEngine,
                userSettings, githubAuthSettings,
                folders, new NullNuKeeperLogger());
            return engine;
        }
    }
}
