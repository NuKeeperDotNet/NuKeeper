using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Output;
using NuKeeper.Commands;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Commands
{
    public class CommandBaseTests
    {
        [Test]
        public async Task EmptyFileResultsInDefaultSettings()
        {
            var fileSettings = FileSettings.Empty();

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings, Is.Not.Null);

            Assert.That(settings.PackageFilters, Is.Not.Null);
            Assert.That(settings.PackageFilters.MinimumAge, Is.EqualTo(TimeSpan.FromDays(7)));
            Assert.That(settings.PackageFilters.Excludes, Is.Null);
            Assert.That(settings.PackageFilters.Includes, Is.Null);
            Assert.That(settings.PackageFilters.MaxPackageUpdates, Is.EqualTo(0));

            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
            Assert.That(settings.UserSettings.OutputDestination, Is.EqualTo(OutputDestination.Console));
            Assert.That(settings.UserSettings.OutputFormat, Is.EqualTo(OutputFormat.Text));
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(0));
            Assert.That(settings.UserSettings.ConsolidateUpdatesInSinglePullRequest, Is.False);

            Assert.That(settings.BranchSettings, Is.Not.Null);
            Assert.That(settings.BranchSettings.BranchNamePrefix, Is.Null);
            Assert.That(settings.BranchSettings.DeleteBranchAfterMerge, Is.EqualTo(false));

            Assert.That(settings.SourceControlServerSettings.IncludeRepos, Is.Null);
            Assert.That(settings.SourceControlServerSettings.ExcludeRepos, Is.Null);
        }

        [Test]
        public async Task NuGetConfigPathOnCommandLine()
        {
            var fileSettings = new FileSettings();

            var settings = await CaptureSettings(fileSettings, nugetConfigFile: "via-cli.config");

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(),
                Is.EqualTo("http://source-from-nuget-config-via-cli.nuget.org/"));
        }

        [Test]
        public async Task NuGetConfigPathInFile()
        {
            var fileSettings = new FileSettings
            {
                NuGetConfigPath = Path.Combine("NuGetConfigs", "via-file.config")
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(),
                Is.EqualTo("http://source-from-nuget-config-via-file.nuget.org/"));
        }

        [Test]
        public async Task NuGetConfigPathOnCommandLineWillReplaceFileNuGetConfigPath()
        {
            var fileSettings = new FileSettings
            {
                NuGetConfigPath = Path.Combine("NuGetConfigs", "via-file.config")
            };

            var settings = await CaptureSettings(fileSettings, nugetConfigFile: "via-cli.config");

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(),
                Is.EqualTo("http://source-from-nuget-config-via-cli.nuget.org/"));
        }

        [Test]
        public async Task SourcesOnCommandLine()
        {
            var fileSettings = new FileSettings();

            var settings = await CaptureSettings(fileSettings, sources: new[] {"http://cli-source.nuget.org/"});

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(), Is.EqualTo("http://cli-source.nuget.org/"));
        }

        [Test]
        public async Task SourcesInFile()
        {
            var fileSettings = new FileSettings
            {
                Sources = new List<string> {"http://file-source.nuget.org/"}
            };

            var settings = await CaptureSettings(fileSettings);

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(), Is.EqualTo("http://file-source.nuget.org/"));
        }

        [Test]
        public async Task SourcesOnCommandLineWillReplaceFileSources()
        {
            var fileSettings = new FileSettings
            {
                Sources = new List<string> {"http://file-source.nuget.org/"}
            };

            var settings = await CaptureSettings(fileSettings, sources: new[] {"http://cli-source.nuget.org/"});

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(), Is.EqualTo("http://cli-source.nuget.org/"));
        }

        [Test]
        public async Task SourcesOnCommandLineWillBeAppendedToNuGetConfigInFile()
        {
            var fileSettings = new FileSettings
            {
                NuGetConfigPath = Path.Combine("NuGetConfigs", "via-file.config")
            };

            var settings = await CaptureSettings(fileSettings, sources: new[] {"http://cli-source.nuget.org/"});

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(),
                Is.EqualTo("http://source-from-nuget-config-via-file.nuget.org/, http://cli-source.nuget.org/"));
        }

        [Test]
        public async Task SourcesInFileWillBeAppendedToNuGetConfigOnCommandLine()
        {
            var fileSettings = new FileSettings
            {
                Sources = new List<string> {"http://file-source.nuget.org/"}
            };

            var settings = await CaptureSettings(fileSettings, nugetConfigFile: "via-cli.config");

            Assert.That(settings.UserSettings.NuGetSources, Is.Not.Null);
            Assert.That(settings.UserSettings.NuGetSources.ToString(),
                Is.EqualTo("http://source-from-nuget-config-via-cli.nuget.org/, http://file-source.nuget.org/"));
        }

        private static async Task<SettingsContainer> CaptureSettings(
            FileSettings settingsIn,
            string nugetConfigFile = null,
            IEnumerable<string> sources = null)
        {
            var logger = Substitute.For<IConfigureLogger>();
            var fileSettings = Substitute.For<IFileSettingsCache>();
            fileSettings.GetSettings().Returns(settingsIn);

            var command = new TestCommand(logger, fileSettings)
            {
                Sources = sources?.ToList(),
                NuGetConfigPath = nugetConfigFile != null ? Path.Combine("NuGetConfigs", nugetConfigFile) : null
            };

            await command.OnExecute();

            return command.LastRunSettings;
        }

        private class TestCommand : CommandBase
        {
            public SettingsContainer LastRunSettings { get; private set; }

            public TestCommand(IConfigureLogger logger, IFileSettingsCache fileSettingsCache) : base(logger,
                fileSettingsCache)
            {
            }

            protected override Task<int> Run(SettingsContainer settings)
            {
                LastRunSettings = settings;
                return Task.FromResult(0);
            }
        }
    }
}
