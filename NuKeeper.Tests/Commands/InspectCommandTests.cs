using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Commands;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Local;
using NUnit.Framework;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class InspectCommandTests
    {
        [Test]
        public async Task ShouldCallEngineAndSucceed()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.Get().Returns(FileSettings.Empty());

            var command = new InspectCommand(engine, logger, fileSettings);

            var status = await command.OnExecute();

            Assert.That(status, Is.EqualTo(0));
            await engine
                .Received(1)
                .Run(Arg.Any<SettingsContainer>(), false);
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
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(0));

            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
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
        public async Task InvalidMaxAgeWillFail()
        {
            var fileSettings = new FileSettings
            {
                Age = "fish"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Null);
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
        public async Task LogLevelIsNormalByDefault()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.Get().Returns(FileSettings.Empty());

            var command = new InspectCommand(engine, logger, fileSettings);

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Normal, string.Empty);
        }

        [Test]
        public async Task ShouldSetLogLevelFromCommand()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.Get().Returns(FileSettings.Empty());

            var command = new InspectCommand(engine, logger, fileSettings);
            command.Verbosity = LogLevel.Minimal;

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Minimal, LogDestination.Console, String.Empty);
        }

        [Test]
        public async Task ShouldSetLogLevelFromFile()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            var settings = new FileSettings
            {
                Verbosity = LogLevel.Detailed
            };

            fileSettings.Get().Returns(settings);

            var command = new InspectCommand(engine, logger, fileSettings);

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Detailed, string.Empty);
        }

        [Test]
        public async Task CommandLineLogLevelOverridesFile()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            var settings = new FileSettings
            {
                Verbosity = LogLevel.Detailed
            };

            fileSettings.Get().Returns(settings);

            var command = new InspectCommand(engine, logger, fileSettings);
            command.Verbosity = LogLevel.Minimal;

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Minimal, string.Empty);
        }

        public async Task<SettingsContainer> CaptureSettings(FileSettings settingsIn)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<ILocalEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x), false);


            fileSettings.Get().Returns(settingsIn);

            var command = new InspectCommand(engine, logger, fileSettings);

            await command.OnExecute();

            return settingsOut;
        }
    }
}
