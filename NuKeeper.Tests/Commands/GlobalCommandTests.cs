using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Commands;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NUnit.Framework;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class GlobalCommandTests
    {
        [Test]
        public async Task ShouldCallEngineAndNotSucceedWithoutParams()
        {
            var engine = Substitute.For<IGitHubEngine>();
            var logger = Substitute.For<IConfigureLogLevel>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.Get().Returns(FileSettings.Empty());

            var command = new GlobalCommand(engine, logger, fileSettings);

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
            var logger = Substitute.For<IConfigureLogLevel>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.Get().Returns(FileSettings.Empty());

            var command = new GlobalCommand(engine, logger, fileSettings);
            command.GitHubToken = "testToken";
            command.Include = "testRepos";
            command.GithubApiEndpoint = "http://github.contoso.com/api";

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
            Assert.That(settings.SourceControlServerSettings.Repository, Is.Null);
            Assert.That(settings.SourceControlServerSettings.OrganisationName, Is.Null);
        }

        [Test]
        public async Task EmptyFileResultsInRequiredParams()
        {
            var fileSettings = FileSettings.Empty();

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.Includes, Is.Not.Null);
            Assert.That(settings.PackageFilters.Includes.ToString(), Is.EqualTo("testRepos"));
            Assert.That(settings.GithubAuthSettings.Token, Is.EqualTo("testToken"));
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings.ApiBase.ToString(), Is.EqualTo("http://github.contoso.com/api/"));
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
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(3));

            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));

            Assert.That(settings.SourceControlServerSettings.Scope, Is.EqualTo(ServerScope.Global));
            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Null);
        }

        [Test]
        public async Task WillReadApiFromFile()
        {
            var fileSettings = new FileSettings
            {
                Api = "http://github.fish.com/api"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.EqualTo(new Uri("http://github.fish.com/api/")));
        }

        [Test]
        public async Task WillReadLabelFromFile()
        {
            var fileSettings = new FileSettings
            {
                Label = "testLabel"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Labels, Is.Not.Null);
            Assert.That(settings.SourceControlServerSettings.Labels, Has.Count.EqualTo(2));
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

        public async Task<SettingsContainer> CaptureSettings(FileSettings settingsIn)
        {
            var logger = Substitute.For<IConfigureLogLevel>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<IGitHubEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x));


            fileSettings.Get().Returns(settingsIn);

            var command = new GlobalCommand(engine, logger, fileSettings);
            command.GitHubToken = "testToken";

            if (string.IsNullOrEmpty(settingsIn.Api))
            {
                command.GithubApiEndpoint = "http://github.contoso.com/api";
            }

            if (string.IsNullOrEmpty(settingsIn.Include))
            {
                command.Include = "testRepos";
            }

            await command.OnExecute();

            return settingsOut;
        }
    }
}
