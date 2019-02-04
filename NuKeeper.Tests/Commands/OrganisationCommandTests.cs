using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Output;
using NuKeeper.Commands;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Collaboration;
using NuKeeper.GitHub;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class OrganisationCommandTests
    {
        private static CollaborationFactory GetCollaborationFactory()
        {
            return new CollaborationFactory(
                new ISettingsReader[] {new GitHubSettingsReader()},
                Substitute.For<INuKeeperLogger>()
            );
        }

        [Test]
        public async Task ShouldCallEngineAndNotSucceedWithoutParams()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(FileSettings.Empty());
            var collaborationFactory = GetCollaborationFactory();

            var command = new OrganisationCommand(engine, logger, fileSettings, collaborationFactory);

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

            var collaborationFactory = GetCollaborationFactory();

            var command = new OrganisationCommand(engine, logger, fileSettings, collaborationFactory);
            command.PersonalAccessToken = "abc";
            command.GithubOrganisationName = "testOrg";

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(0));
            await engine
                .Received(1)
                .Run(Arg.Any<SettingsContainer>());
        }

        [Test]
        public async Task ShouldPopulateGithubSettings()
        {
            var fileSettings = FileSettings.Empty();

            var (settings, platformSettings) = await CaptureSettings(fileSettings);

            Assert.That(platformSettings, Is.Not.Null);
            Assert.That(platformSettings.Token, Is.Not.Null);
            Assert.That(platformSettings.Token, Is.EqualTo("testToken"));

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Scope, Is.EqualTo(ServerScope.Organisation));
            Assert.That(settings.SourceControlServerSettings.Repository, Is.Null);
            Assert.That(settings.SourceControlServerSettings.OrganisationName, Is.EqualTo("testOrg"));
        }

        [Test]
        public async Task EmptyFileResultsInDefaultSettings()
        {
            var fileSettings = FileSettings.Empty();

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);

            Assert.That(settings.PackageFilters.MinimumAge, Is.EqualTo(TimeSpan.FromDays(7)));
            Assert.That(settings.PackageFilters.Excludes, Is.Null);
            Assert.That(settings.PackageFilters.Includes, Is.Null);
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(3));

            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.Console));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Text));

            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(10));

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
        public async Task WillReadRepoFiltersFromFile()
        {
            var fileSettings = new FileSettings
            {
                IncludeRepos = "foo",
                ExcludeRepos = "bar"
            };

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos.ToString(), Is.EqualTo("foo"));
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos.ToString(), Is.EqualTo("bar"));
        }

        [Test]
        public async Task WillReadMaxUpdateFromFile()
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
        public async Task WillReadMaxRepoFromFile()
        {
            var fileSettings = new FileSettings
            {
                MaxRepo = 42
            };

            var (settings, _) = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(42));
        }

        [Test]
        public async Task CommandLineWillOverrideIncludeRepo()
        {
            var fileSettings = new FileSettings
            {
                IncludeRepos = "foo",
                ExcludeRepos = "bar"
            };

            var (settings, _) = await CaptureSettings(fileSettings, true);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos.ToString(), Is.EqualTo("IncludeFromCommand"));
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos.ToString(), Is.EqualTo("bar"));
        }

        [Test]
        public async Task CommandLineWillOverrideExcludeRepo()
        {
            var fileSettings = new FileSettings
            {
                IncludeRepos = "foo",
                ExcludeRepos = "bar"
            };

            var (settings, _) = await CaptureSettings(fileSettings, false, true);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos.ToString(), Is.EqualTo("foo"));
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos.ToString(), Is.EqualTo("ExcludeFromCommand"));
        }

        [Test]
        public async Task CommandLineWillOverrideForkMode()
        {
            var (_, platformSettings) = await CaptureSettings(FileSettings.Empty(), false, false, null, ForkMode.PreferSingleRepository);

            Assert.That(platformSettings, Is.Not.Null);
            Assert.That(platformSettings.ForkMode, Is.Not.Null);
            Assert.That(platformSettings.ForkMode, Is.EqualTo(ForkMode.PreferSingleRepository));
        }

        [Test]
        public async Task CommandLineWillOverrideMaxRepo()
        {
            var fileSettings = new FileSettings
            {
                MaxRepo = 12,
            };

            var (settings, _) = await CaptureSettings(fileSettings, false, true, 22);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(22));
        }

        public static async Task<(SettingsContainer fileSettings, CollaborationPlatformSettings platformSettings)> CaptureSettings(
            FileSettings settingsIn,
            bool addCommandRepoInclude = false,
            bool addCommandRepoExclude = false,
            int? maxRepo = null,
            ForkMode? forkMode = null)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<ICollaborationEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x));

            fileSettings.GetSettings().Returns(settingsIn);

            var collaborationFactory = GetCollaborationFactory();

            var command = new OrganisationCommand(engine, logger, fileSettings, collaborationFactory);
            command.PersonalAccessToken = "testToken";
            command.GithubOrganisationName = "testOrg";

            if (addCommandRepoInclude)
            {
                command.IncludeRepos = "IncludeFromCommand";
            }

            if (addCommandRepoExclude)
            {
                command.ExcludeRepos = "ExcludeFromCommand";
            }

            if (forkMode != null)
            {
                command.ForkMode = forkMode;
            }

            command.AllowedMaxRepositoriesChangedChange = maxRepo;

            await command.OnExecute();

            return (settingsOut, collaborationFactory.Settings);
        }
    }
}
