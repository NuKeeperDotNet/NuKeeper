using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.Output;
using NuKeeper.Collaboration;
using NuKeeper.Commands;
using NuKeeper.Engine;
using NuKeeper.Git;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class RepositoryCommandTests
    {
        [Test]
        public async Task ShouldCallEngineAndNotSucceedWithoutParams()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> {settingReader};
            var collaborationFactory = GetCollaborationFactory(settingsReaders);

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders);

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(-1));
            await engine
                .DidNotReceive()
                .Run(Arg.Any<SettingsContainer>());
        }

        [Test]
        public async Task ShouldCallEngineAndSucceedWithRequiredGithubParams()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> {settingReader};
            var collaborationFactory = GetCollaborationFactory(settingsReaders);

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders)
            {
                PersonalAccessToken = "abc",
                RepositoryUri = "http://github.com/abc/abc"
            };

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(0));
            await engine
                .Received(1)
                .Run(Arg.Any<SettingsContainer>());
        }

        [Test]
        public async Task ShouldInitialiseCollaborationFactory()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> { settingReader };
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.Settings.Returns(new CollaborationPlatformSettings());

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders)
            {
                PersonalAccessToken = "abc",
                RepositoryUri = "http://github.com/abc/abc",
                ForkMode = ForkMode.PreferSingleRepository
            };

            await command.OnExecute();

            collaborationFactory
                .Received(1)
                .Initialise(
                    Arg.Is(new Uri("https://api.github.com")),
                    Arg.Is("abc"),
                    Arg.Is<ForkMode?>(ForkMode.PreferSingleRepository),
                    Arg.Is((Platform?)null));
        }

        [Test]
        public async Task ShouldInitialiseForkModeFromFile()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(
                new FileSettings
                {
                    ForkMode = ForkMode.PreferFork
                });

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> { settingReader };
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.Settings.Returns(new CollaborationPlatformSettings());

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders)
            {
                PersonalAccessToken = "abc",
                RepositoryUri = "http://github.com/abc/abc",
                ForkMode = null
            };

            await command.OnExecute();

            collaborationFactory
                .Received(1)
                .Initialise(
                    Arg.Is(new Uri("https://api.github.com")),
                    Arg.Is("abc"),
                    Arg.Is<ForkMode?>(ForkMode.PreferFork),
                    Arg.Is((Platform?)null));
        }

        [Test]
        public async Task ShouldInitialisePlatformFromFile()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(
                new FileSettings
                {
                    Platform = Platform.BitbucketLocal
                });

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> { settingReader };
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.Settings.Returns(new CollaborationPlatformSettings());

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders)
            {
                PersonalAccessToken = "abc",
                RepositoryUri = "http://github.com/abc/abc",
                ForkMode = null
            };

            await command.OnExecute();

            collaborationFactory
                .Received(1)
                .Initialise(
                    Arg.Is(new Uri("https://api.github.com")),
                    Arg.Is("abc"),
                    Arg.Is((ForkMode?)null),
                    Arg.Is((Platform?)Platform.BitbucketLocal));
        }

        [Test]
        public async Task ShouldPopulateSourceControlServerSettings()
        {
            var fileSettings = FileSettings.Empty();

            var (settings, platformSettings) = await CaptureSettings(fileSettings);

            Assert.That(platformSettings, Is.Not.Null);
            Assert.That(platformSettings.Token, Is.Not.Null);
            Assert.That(platformSettings.Token, Is.EqualTo("testToken"));

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Scope, Is.EqualTo(ServerScope.Repository));
            Assert.That(settings.SourceControlServerSettings.OrganisationName, Is.Null);
        }

        [Test]
        public async Task EmptyFileResultsInDefaultSettings()
        {
            var fileSettings = FileSettings.Empty();

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);

            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.MinimumAge, Is.EqualTo(TimeSpan.FromDays(7)));
            Assert.That(settings.PackageFilters.Excludes, Is.Null);
            Assert.That(settings.PackageFilters.Includes, Is.Null);
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(3));

            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.Console));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Text));
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(1));
            Assert.That(settings.UserSettings.ConsolidateUpdatesInSinglePullRequest, Is.False);

            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Null);
        }

        [Test]
        public async Task WillReadApiFromFile()
        {
            var fileSettings = new FileSettings
            {
                Api = "http://github.contoso.com/"
            };

            var (_, platformSettings) = await CaptureSettings(fileSettings);

            Assert.That(platformSettings, Is.Not.Null);
            Assert.That(platformSettings.BaseApiUrl, Is.Not.Null);
            Assert.That(platformSettings.BaseApiUrl, Is.EqualTo(new Uri("http://github.contoso.com/")));
        }

        [Test]
        public async Task WillReadLabelFromFile()
        {
            var fileSettings = new FileSettings
            {
                Label = new List<string> {"testLabel"}
            };

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Labels, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Labels, Has.Count.EqualTo(1));
            Assert.That(settings.SourceControlServerSettings.Labels, Does.Contain("testLabel"));
        }

        [Test]
        public async Task WillReadMaxPackageUpdatesFromFile()
        {
            var fileSettings = new FileSettings
            {
                MaxPackageUpdates = 42
            };

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(42));
        }

        [Test]
        public async Task WillReadConsolidateFromFile()
        {
            var fileSettings = new FileSettings
            {
                Consolidate = true
            };

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings.ConsolidateUpdatesInSinglePullRequest, Is.True);
        }

        [Test]
        public async Task WillNotReadMaxRepoFromFile()
        {
            var fileSettings = new FileSettings
            {
                MaxRepo = 42
            };

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(1));
        }

        [Test]
        public async Task MaxPackageUpdatesFromCommandLineOverridesFiles()
        {
            var fileSettings = new FileSettings
            {
                MaxPackageUpdates = 42
            };

            var (settings, _) = await CaptureSettings(fileSettings, false, 101);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(101));
        }

        [Test]
        public async Task LabelsOnCommandLineWillReplaceFileLabels()
        {
            var fileSettings = new FileSettings
            {
                Label = new List<string> {"testLabel"}
            };

            var (settings, _) = await CaptureSettings(fileSettings, true);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Labels, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Labels, Has.Count.EqualTo(2));
            Assert.That(settings.SourceControlServerSettings.Labels, Does.Contain("runLabel1"));
            Assert.That(settings.SourceControlServerSettings.Labels, Does.Contain("runLabel2"));
            Assert.That(settings.SourceControlServerSettings.Labels, Does.Not.Contain("testLabel"));
        }

        public static async Task<(SettingsContainer settingsContainer, CollaborationPlatformSettings platformSettings)> CaptureSettings(
            FileSettings settingsIn,
            bool addLabels = false,
            int? maxPackageUpdates = null)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(settingsIn);

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> {settingReader};
            var collaborationFactory = GetCollaborationFactory(settingsReaders);

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<ICollaborationEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x));

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders);
            command.PersonalAccessToken = "testToken";
            command.RepositoryUri = "http://github.com/test/test";

            if (addLabels)
            {
                command.Label = new List<string> {"runLabel1", "runLabel2"};
            }

            command.MaxPackageUpdates = maxPackageUpdates;

            await command.OnExecute();

            return (settingsOut, collaborationFactory.Settings);
        }

        [Test]
        public async Task ShouldHaveTargetbranchIfParameterIsProvided()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var settingReader = new GitHubSettingsReader();
            var settingsReaders = new List<ISettingsReader> {settingReader};
            var collaborationFactory = GetCollaborationFactory(settingsReaders);

            var command = new RepositoryCommand(engine, logger, fileSettings, collaborationFactory, settingsReaders);

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(-1));
            await engine
                .DidNotReceive()
                .Run(Arg.Any<SettingsContainer>());
        }

        [Test]
        public async Task ShouldNotHaveTargetbranchIfParameterIsProvided()
        {
            var testUri = new Uri("https://github.com");

            var collaborationFactorySubstitute = Substitute.For<ICollaborationFactory>();
            collaborationFactorySubstitute.ForkFinder.FindPushFork(Arg.Any<string>(), Arg.Any<ForkData>()).Returns(Task.FromResult(new ForkData(testUri, "nukeeper","nukeeper")));

            var updater = Substitute.For<IRepositoryUpdater>();
            var gitEngine = new GitRepositoryEngine(updater, collaborationFactorySubstitute, Substitute.For<IFolderFactory>(),
                Substitute.For<INuKeeperLogger>(), Substitute.For<IRepositoryFilter>());

            await gitEngine.Run(new RepositorySettings
            {
                RepositoryUri = testUri,
                TargetBranch = "custombranch",
                RepositoryOwner = "nukeeper",
                RepositoryName = "nukeeper"
            }, new GitUsernamePasswordCredentials()
            {
                Password = "..",
                Username = "nukeeper"
            }, new SettingsContainer() { SourceControlServerSettings = new SourceControlServerSettings()
            {
                Scope = ServerScope.Repository
            }}, null);


            await updater.Received().Run(Arg.Any<IGitDriver>(),
                Arg.Is<RepositoryData>(r => r.DefaultBranch == "custombranch"), Arg.Any<SettingsContainer>());
        }

        private static ICollaborationFactory GetCollaborationFactory(IEnumerable<ISettingsReader> settingReaders = null)
        {
            return new CollaborationFactory(
                settingReaders ?? new ISettingsReader[] { new GitHubSettingsReader() },
                Substitute.For<INuKeeperLogger>()
            );
        }
    }
}
