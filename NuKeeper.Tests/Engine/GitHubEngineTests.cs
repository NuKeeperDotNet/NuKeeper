using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
using NuKeeper.Inspection.Files;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                AuthSettings = MakeAuthSettings(),
                UserSettings = new UserSettings
                {
                    MaxRepositoriesChanged = 1
                },
                SourceControlServerSettings = MakeServerSettings()
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
            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            var repoDiscovery = Substitute.For<IGitHubRepositoryDiscovery>();
            var repoEngine = Substitute.For<IGitHubRepositoryEngine>();
            var folders = Substitute.For<IFolderFactory>();

            var user = RepositoryBuilder.MakeUser("http://test.user.com");
            collaborationPlatform.GetCurrentUser().Returns(new User(user.Login, user.Name, user.Email));

            repoDiscovery.GetRepositories(Arg.Any<ICollaborationPlatform>(), Arg.Any<SourceControlServerSettings>())
                .Returns(repos);

            repoEngine.Run(null, null, null, null).ReturnsForAnyArgs(repoEngineResult);

            var engine = new GitHubEngine(collaborationPlatform, repoDiscovery, repoEngine,
                folders, Substitute.For<INuKeeperLogger>());
            return engine;
        }

        private static SettingsContainer MakeSettings()
        {
            return new SettingsContainer
            {
                AuthSettings = MakeAuthSettings(),
                UserSettings = MakeUserSettings(),
                SourceControlServerSettings = MakeServerSettings()
            };
        }

        private static SourceControlServerSettings MakeServerSettings()
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

        private static AuthSettings MakeAuthSettings()
        {
            return new AuthSettings(new Uri("http://foo.com"), "token123");
        }
    }
}
