using System;
using System.Linq;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NUnit.Framework;

namespace NuKeeper.Tests.Configuration
{
    [TestFixture]
    public class SettingsParserTests
    {
        [Test]
        public void UnPopulatedConfigIsNotValid()
        {
            var raw = new RawConfiguration();
            var settings = SettingsParser.ParseToSettings(raw);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void ValidRepoConfigIsParsed()
        {
            var raw = ValidRepoSettings();

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Repository));
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.UserSettings.ForkMode, Is.EqualTo(ForkMode.PreferFork));
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
            Assert.That(settings.UserSettings.MaxPullRequestsPerRepository, Is.EqualTo(10));
        }

        [Test]
        public void ValidOrgConfigIsParsed()
        {
            var raw = ValidOrgSettings();

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Organisation));
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.UserSettings.ForkMode, Is.EqualTo(ForkMode.PreferFork));
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
            Assert.That(settings.UserSettings.MaxPullRequestsPerRepository, Is.EqualTo(10));
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(20));
        }

        [Test]
        public void ValidInspectConfigIsParsed()
        {
            var raw = ValidInspectSettings();

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
        }

        [Test]
        public void ValidInspectConfigWithDirIsParsed()
        {
            var raw = ValidInspectSettings();
            raw.Dir = "c:\\temp";

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
            Assert.That(settings.UserSettings.Directory, Is.EqualTo("c:\\temp"));
        }

        [Test]
        public void ValidInspectConfigWithoutSourcesIsParsedToDefault()
        {
            var raw = ValidInspectSettings();

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
        }

        [Test]
        public void ValidInspectConfigWithSourcesIsParsed()
        {
            var raw = ValidInspectSettings();
            raw.NuGetSources = "https://foo;file://fish";

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
            var sources = settings.UserSettings.NuGetSources;
            Assert.That(sources.Items.Count, Is.EqualTo(2));
            Assert.That(sources.Items.First(), Is.EqualTo("https://foo/"));
            Assert.That(sources.Items.Skip(1).First(), Is.EqualTo("file://fish/"));
        }

        [Test]
        public void ValidUpdateConfigIsParsed()
        {
            var raw = ValidUpdateSettings();

            var settings = SettingsParser.ParseToSettings(raw);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Update));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
        }

        private static RawConfiguration ValidRepoSettings()
        {
            return new RawConfiguration
            {
                GithubApiEndpoint = new Uri("https://api.github.com"),
                NuGetSources = null,
                Mode = "repository",
                GithubRepositoryUri = new Uri("https://github.com/NuKeeperDotNet/NuKeeper"),
                GithubToken = "abc123",
                AllowedChange = VersionChange.Major,
                LogLevel = LogLevel.Info,
                ForkMode = ForkMode.PreferFork,
                ReportMode = ReportMode.Off,
                Labels = "nukeeper",
                MaxPullRequestsPerRepository = 10
            };
        }

        private static RawConfiguration ValidOrgSettings()
        {
            return new RawConfiguration
            {
                GithubApiEndpoint = new Uri("https://api.github.com"),
                NuGetSources = null,
                Mode = "organisation",
                GithubOrganisationName = "NuKeeperDotNet",
                GithubToken = "abc123",
                AllowedChange = VersionChange.Major,
                LogLevel = LogLevel.Info,
                ForkMode = ForkMode.PreferFork,
                ReportMode = ReportMode.Off,
                Labels = "nukeeper",
                MaxPullRequestsPerRepository = 10,
                MaxRepositoriesChanged = 20
            };
        }

        private static RawConfiguration ValidInspectSettings()
        {
            return new RawConfiguration
            {
                NuGetSources = null,
                Mode = "inspect",
                LogLevel = LogLevel.Info,
                ReportMode = ReportMode.Off,
                Labels = "nukeeper"
            };
        }

        private static RawConfiguration ValidUpdateSettings()
        {
            return new RawConfiguration
            {
                NuGetSources = null,
                Mode = "update",
                LogLevel = LogLevel.Info,
                ReportMode = ReportMode.Off,
                Labels = "nukeeper"
            };
        }

        private static void AssertSettingsNotNull(SettingsContainer settings)
        {
            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings, Is.Not.Null);
            Assert.That(settings.GithubAuthSettings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
        }
    }
}
