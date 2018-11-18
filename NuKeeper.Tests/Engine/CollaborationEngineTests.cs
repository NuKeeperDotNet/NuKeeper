using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
using NuKeeper.Inspection.Files;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Collaboration;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class CollaborationEngineTests
    {
        [Test]
        public async Task SuccessCaseWithNoRepos()
        {
            var engine = MakeCollaborationEngine(
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
            var engine = MakeCollaborationEngine(oneRepo);

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
            var engine = MakeCollaborationEngine(repos);

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
            var engine = MakeCollaborationEngine(2, repos);

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
            var engine = MakeCollaborationEngine(0, repos);

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

            var engine = MakeCollaborationEngine(1, repos);

            var settings = new SettingsContainer
            {
                UserSettings = new UserSettings
                {
                    MaxRepositoriesChanged = 1
                },
                SourceControlServerSettings = MakeServerSettings()
            };

            var count = await engine.Run(settings);

            Assert.That(count, Is.EqualTo(1));
        }

        private static CollaborationEngine MakeCollaborationEngine(
            List<RepositorySettings> repos)
        {
            return MakeCollaborationEngine(1, repos);
        }

        private static CollaborationEngine MakeCollaborationEngine(
            int repoEngineResult,
            List<RepositorySettings> repos)
        {
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            var repoEngine = Substitute.For<IGitRepositoryEngine>();
            var folders = Substitute.For<IFolderFactory>();

            var user = new User("testUser", "Testy", "testuser@test.com");
            collaborationFactory.CollaborationPlatform.GetCurrentUser().Returns(user);

            collaborationFactory.Settings.Returns(new CollaborationPlatformSettings());

            collaborationFactory.RepositoryDiscovery.GetRepositories(Arg.Any<SourceControlServerSettings>()).Returns(repos);

            repoEngine.Run(null, null, null, null).ReturnsForAnyArgs(repoEngineResult);

            var engine = new CollaborationEngine(collaborationFactory, repoEngine,
                folders, Substitute.For<INuKeeperLogger>());
            return engine;
        }

        private static SettingsContainer MakeSettings()
        {
            return new SettingsContainer
            {
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
            return new UserSettings {MaxRepositoriesChanged = int.MaxValue};
        }
    }
}
