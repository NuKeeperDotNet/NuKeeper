using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.Output;
using NuKeeper.Commands;
using NuKeeper.Inspection.Logging;
using NuKeeper.Local;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

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

            fileSettings.GetSettings().Returns(FileSettings.Empty());

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
            Assert.That(settings.BranchSettings, Is.Not.Null);

            Assert.That(settings.PackageFilters.MinimumAge, Is.EqualTo(TimeSpan.FromDays(7)));
            Assert.That(settings.PackageFilters.Excludes, Is.Null);
            Assert.That(settings.PackageFilters.Includes, Is.Null);
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(0));

            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.Console));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Text));

            Assert.That(settings.BranchSettings.BranchNameTemplate, Is.Null);
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
        public async Task WillReadBranchNamePrefixFromFile()
        {
            var testTemplate = "nukeeper/MyBranch";

            var fileSettings = new FileSettings
            {
                BranchNameTemplate = testTemplate
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.BranchSettings, Is.Not.Null);
            Assert.That(settings.BranchSettings.BranchNameTemplate, Is.EqualTo(testTemplate));
        }

        [Test]
        public async Task LogLevelIsNormalByDefault()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var command = new InspectCommand(engine, logger, fileSettings);

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Normal, LogDestination.Console, Arg.Any<string>());
        }

        [Test]
        public async Task ShouldSetLogLevelFromCommand()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            fileSettings.GetSettings().Returns(FileSettings.Empty());

            var command = new InspectCommand(engine, logger, fileSettings);
            command.Verbosity = LogLevel.Minimal;

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Minimal, LogDestination.Console, Arg.Any<string>());
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

            fileSettings.GetSettings().Returns(settings);

            var command = new InspectCommand(engine, logger, fileSettings);

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Detailed, LogDestination.Console, Arg.Any<string>());
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

            fileSettings.GetSettings().Returns(settings);

            var command = new InspectCommand(engine, logger, fileSettings);
            command.Verbosity = LogLevel.Minimal;

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Minimal, LogDestination.Console, Arg.Any<string>());
        }

        [Test]
        public async Task LogToFileBySettingFileName()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            var settings = FileSettings.Empty();

            fileSettings.GetSettings().Returns(settings);

            var command = new InspectCommand(engine, logger, fileSettings);
            command.LogFile = "somefile.log";

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Normal, LogDestination.File, "somefile.log");
        }

        [Test]
        public async Task LogToFileBySettingLogDestination()
        {
            var engine = Substitute.For<ILocalEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            var settings = FileSettings.Empty();

            fileSettings.GetSettings().Returns(settings);

            var command = new InspectCommand(engine, logger, fileSettings);
            command.LogDestination = LogDestination.File;

            await command.OnExecute();

            logger
                .Received(1)
                .Initialise(LogLevel.Normal, LogDestination.File, "nukeeper.log");
        }

        [Test]
        public async Task ShouldSetOutputOptionsFromFile()
        {
            var fileSettings = new FileSettings
            {
                OutputDestination = OutputDestination.File,
                OutputFormat = OutputFormat.Csv,
                OutputFileName = "foo.csv"
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.File));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Csv));
            Assert.That(settings.UserSettings.OutputFileName, Is.EqualTo("foo.csv"));
        }

        [Test]
        public async Task WhenFileNameIsExplicit_ShouldDefaultOutputDestToFile()
        {
            var fileSettings = new FileSettings
            {
                OutputDestination = null,
                OutputFormat = OutputFormat.Csv
            };

            var settings = await CaptureSettings(fileSettings, null, null, "foo.csv");

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.File));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Csv));
            Assert.That(settings.UserSettings.OutputFileName, Is.EqualTo("foo.csv"));
        }

        [Test]
        public async Task WhenFileNameIsExplicit_ShouldKeepOutputDest()
        {
            var fileSettings = new FileSettings
            {
                OutputDestination = OutputDestination.Off,
                OutputFormat = OutputFormat.Csv
            };

            var settings = await CaptureSettings(fileSettings, null, null, "foo.csv");

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.Off));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Csv));
            Assert.That(settings.UserSettings.OutputFileName, Is.EqualTo("foo.csv"));
        }

        [Test]
        public async Task ShouldSetOutputOptionsFromCommand()
        {
            var settingsOut = await CaptureSettings(FileSettings.Empty(),
                OutputDestination.File,
                OutputFormat.Csv);

            Assert.That(settingsOut.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.File));
            Assert.That(settingsOut.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Csv));
        }

        public static async Task<SettingsContainer> CaptureSettings(FileSettings settingsIn,
            OutputDestination? outputDestination = null,
            OutputFormat? outputFormat = null,
            string outputFileName = null)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();

            SettingsContainer settingsOut = null;
            var engine = Substitute.For<ILocalEngine>();
            await engine.Run(Arg.Do<SettingsContainer>(x => settingsOut = x), false);


            fileSettings.GetSettings().Returns(settingsIn);

            var command = new InspectCommand(engine, logger, fileSettings);

            if (outputDestination.HasValue)
            {
                command.OutputDestination = outputDestination.Value;
            }
            if (outputFormat.HasValue)
            {
                command.OutputFormat = outputFormat.Value;
            }

            if (!string.IsNullOrWhiteSpace(outputFileName))
            {
                command.OutputFileName = outputFileName;
            }

            await command.OnExecute();

            return settingsOut;
        }
    }
}
