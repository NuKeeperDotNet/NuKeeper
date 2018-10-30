using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Output;
using NuKeeper.Commands;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class OrganisationCommandTests
    {
        [Test]
        public async Task ShouldCallEngineAndNotSucceedWithoutParams()
        {
            var engine = Substitute.For<IGitHubEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var command = new OrganisationCommand(engine, logger, fileSettings);

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(-1));
            await engine
                .DidNotReceive()
                .Run(Arg.Any<SettingsContainer>());
        }

        [Test]
        public async Task ShouldCallEngineAndSucceedWithRequiredGithubParams()
        {
            var engine = Substitute.For<IGitHubEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var command = new OrganisationCommand(engine, logger, fileSettings);
            command.GitHubToken = "abc";
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

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings.Token, Is.EqualTo("testToken"));

            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Scope, Is.EqualTo(ServerScope.Organisation));
            Assert.That(settings.SourceControlServerSettings.Repository, Is.Null);
            Assert.That(settings.SourceControlServerSettings.OrganisationName, Is.EqualTo("testOrg"));
        }

        [Test]
        public async Task EmptyFileResultsInDefaultSettings()
        {
            var fileSettings = FileSettings.Empty();

            var settings = await CaptureSettings(fileSettings);

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
                Api = "http://github.contoso.com/api"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.EqualTo(new Uri("http://github.contoso.com/api/")));
        }

        [Test]
        public async Task WillReadLabelFromFile()
        {
            var fileSettings = new FileSettings
            {
                Label = new List<string> { "testLabel" }
            };

            var settings = await CaptureSettings(fileSettings);

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

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos.ToString(), Is.EqualTo("foo"));
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos.ToString(), Is.EqualTo("bar"));
        }

        [Test]
        public async Task WillReadMaxPrFromFile()
        {
            var fileSettings = new FileSettings
            {
                MaxPr = 42
            };

            var settings = await CaptureSettings(fileSettings);

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

            var settings = await CaptureSettings(fileSettings);

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

            var settings = await CaptureSettings(fileSettings, true, false);

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

            var settings = await CaptureSettings(fileSettings, false, true);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.IncludeRepos.ToString(), Is.EqualTo("foo"));
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos.ToString(), Is.EqualTo("ExcludeFromCommand"));
        }

        [Test]
        public async Task CommandLineWillOverrideMaxRepo()
        {
            var fileSettings = new FileSettings
            {
                MaxRepo =  12,
            };

            var settings = await CaptureSettings(fileSettings, false, true, 22);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(22));
        }

        public static async Task<SettingsContainer> CaptureSettings(
            FileSettings settingsIn,
            bool addCommandRepoInclude = false,
            bool addCommandRepoExclude = false,
            int? maxRepo = null)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<IGitHubEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x));


            fileSettings.GetSettings().Returns(settingsIn);

            var command = new OrganisationCommand(engine, logger, fileSettings);
            command.GitHubToken = "testToken";
            command.GithubOrganisationName = "testOrg";

            if (addCommandRepoInclude)
            {
                command.IncludeRepos = "IncludeFromCommand";
            }

            if (addCommandRepoExclude)
            {
                command.ExcludeRepos = "ExcludeFromCommand";
            }

            command.AllowedMaxRepositoriesChangedChange = maxRepo;

            await command.OnExecute();

            return settingsOut;
        }
    }
}
