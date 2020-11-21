using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Collaboration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Files;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class CollaborationEngineTests
    {
        private IGitRepositoryEngine _repoEngine;
        private ICollaborationFactory _collaborationFactory;
        private IFolderFactory _folderFactory;
        private INuKeeperLogger _logger;
        private List<RepositorySettings> _disoverableRepositories;

        [SetUp]
        public void Initialize()
        {
            _repoEngine = Substitute.For<IGitRepositoryEngine>();
            _collaborationFactory = Substitute.For<ICollaborationFactory>();
            _folderFactory = Substitute.For<IFolderFactory>();
            _logger = Substitute.For<INuKeeperLogger>();
            _disoverableRepositories = new List<RepositorySettings>();

            _collaborationFactory.CollaborationPlatform.GetCurrentUser().Returns(new User("", "", ""));
            _collaborationFactory.Settings.Returns(new CollaborationPlatformSettings());
            _collaborationFactory
                .RepositoryDiscovery
                .GetRepositories(Arg.Any<SourceControlServerSettings>())
                .Returns(_disoverableRepositories);
        }

        [Test]
        public async Task Run_ExceptionWhenUpdatingRepository_StillTreatsOtherRepositories()
        {
            var settings = MakeSettings();
            var repoSettingsOne = MakeRepositorySettingsAndAddAsDiscoverable();
            var repoSettingsTwo = MakeRepositorySettingsAndAddAsDiscoverable();
            _repoEngine
                .When(
                    r => r.Run(
                        repoSettingsOne,
                        Arg.Any<GitUsernamePasswordCredentials>(),
                        Arg.Any<SettingsContainer>(),
                        Arg.Any<User>()
                    )
                )
                .Do(r => throw new Exception());
            var engine = MakeCollaborationEngine();

            try
            {
                await engine.Run(settings);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types

            await _repoEngine
                .Received()
                .Run(
                    repoSettingsTwo,
                    Arg.Any<GitUsernamePasswordCredentials>(),
                    Arg.Any<SettingsContainer>(),
                    Arg.Any<User>()
                );
        }

        [Test]
        public void Run_ExceptionWhenUpdatingRepository_RethrowsException()
        {
            var exceptionMessage = "Try again later";
            var settings = MakeSettings();
            var repoSettingsOne = MakeRepositorySettingsAndAddAsDiscoverable();
            var repoSettingsTwo = MakeRepositorySettingsAndAddAsDiscoverable();
            _repoEngine
                .When(
                    r => r.Run(
                        repoSettingsOne,
                        Arg.Any<GitUsernamePasswordCredentials>(),
                        Arg.Any<SettingsContainer>(),
                        Arg.Any<User>()
                    )
                )
                .Do(r => throw new NuKeeperException(exceptionMessage));
            var engine = MakeCollaborationEngine();

            var ex = Assert.ThrowsAsync<NuKeeperException>(() => engine.Run(settings));

            var innerEx = ex.InnerException as NuKeeperException;
            Assert.IsNotNull(innerEx);
            Assert.AreEqual(exceptionMessage, innerEx.Message);
        }

        [Test]
        public void Run_MultipleExceptionsWhenUpdatingRepositories_AreFlattened()
        {
            var settings = MakeSettings();
            var repoSettingsOne = MakeRepositorySettingsAndAddAsDiscoverable();
            var repoSettingsTwo = MakeRepositorySettingsAndAddAsDiscoverable();
            var repoSettingsThree = MakeRepositorySettingsAndAddAsDiscoverable();
            _repoEngine
                .When(
                    r => r.Run(
                        repoSettingsOne,
                        Arg.Any<GitUsernamePasswordCredentials>(),
                        Arg.Any<SettingsContainer>(),
                        Arg.Any<User>()
                    )
                )
                .Do(r => throw new InvalidOperationException("Repo 1 failed!"));
            _repoEngine
                .When(
                    r => r.Run(
                        repoSettingsThree,
                        Arg.Any<GitUsernamePasswordCredentials>(),
                        Arg.Any<SettingsContainer>(),
                        Arg.Any<User>()
                    )
                )
                .Do(r => throw new TaskCanceledException("Repo 3 failed!"));
            var engine = MakeCollaborationEngine();

            var ex = Assert.ThrowsAsync<NuKeeperException>(() => engine.Run(settings));

            var aggregateEx = ex.InnerException as AggregateException;
            var exceptions = aggregateEx?.InnerExceptions;
            Assert.IsNotNull(aggregateEx);
            Assert.IsNotNull(exceptions);
            Assert.AreEqual(2, exceptions.Count);
            Assert.That(exceptions, Has.One.InstanceOf(typeof(InvalidOperationException)));
            Assert.That(exceptions, Has.One.InstanceOf(typeof(TaskCanceledException)));
        }

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

        private ICollaborationEngine MakeCollaborationEngine()
        {
            return new CollaborationEngine(
                _collaborationFactory,
                _repoEngine,
                _folderFactory,
                _logger
            );
        }

        private RepositorySettings MakeRepositorySettingsAndAddAsDiscoverable()
        {
            var repositorySettings = new RepositorySettings();
            _disoverableRepositories.Add(repositorySettings);
            return repositorySettings;
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
            return new UserSettings { MaxRepositoriesChanged = int.MaxValue };
        }
    }
}
