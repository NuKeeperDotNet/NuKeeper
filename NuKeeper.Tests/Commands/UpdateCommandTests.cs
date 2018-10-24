using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Abstract.Local;
using NuKeeper.Commands;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NUnit.Framework;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class UpdateCommandTests
    {
        [Test]
        public async Task ShouldCallEngineAndSucceed()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var command = new UpdateCommand(engine, logger, fileSettings);

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(0));
            await engine
                .Received(1)
                .Run(Arg.Any<SettingsContainer>(), true);
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
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(1));

            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.Console));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Text));
        }

        [Test]
        public async Task WillReadMaxAgeFromFile()
        {
            var fileSettings = new FileSettings
            {
                Age = "8d"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.MinimumAge, Is.EqualTo(TimeSpan.FromDays(8)));
        }

        [Test]
        public async Task WillReadIncludeExcludeFromFile()
        {
            var fileSettings = new FileSettings
            {
                Include = "foo",
                Exclude = "bar"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.Includes.ToString(), Is.EqualTo("foo"));
            Assert.That(settings.PackageFilters.Excludes.ToString(), Is.EqualTo("bar"));
        }

        [Test]
        public async Task WillReadVersionChangeFromCommandLineOverFile()
        {
            var fileSettings = new FileSettings
            {
                Change = VersionChange.Patch
            };

            var settings = await CaptureSettings(fileSettings, VersionChange.Minor);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Minor));
        }

        [Test]
        public async Task WillReadVersionChangeFromFile()
        {
            var fileSettings = new FileSettings
            {
                Change = VersionChange.Patch
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Patch));
        }

        public static async Task<SettingsContainer> CaptureSettings(FileSettings settingsIn,
            VersionChange? change = null)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<ILocalEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x), true);


            fileSettings.GetSettings().Returns(settingsIn);

            var command = new UpdateCommand(engine, logger, fileSettings);
            command.AllowedChange = change;

            await command.OnExecute();

            return settingsOut;
        }
    }
}
