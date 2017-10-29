using System;
using NuKeeper.Configuration;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
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

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Repository));
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
        }

        [Test]
        public void ValidOrgConfigIsParsed()
        {
            var raw = ValidOrgSettings();

            var settings = SettingsParser.ParseToSettings(raw);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Organisation));
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
        }

        private static RawConfiguration ValidRepoSettings()
        {
            return new RawConfiguration
            {
                GithubApiEndpoint = new Uri("https://api.github.com"),
                NuGetSources = "https://api.nuget.org/v3/index.json",
                Mode = "repository",
                GithubRepositoryUri = new Uri("https://github.com/NuKeeperDotNet/NuKeeper"),
                AllowedChange = VersionChange.Major,
                LogLevel = LogLevel.Info
            };
        }

        private static RawConfiguration ValidOrgSettings()
        {
            return new RawConfiguration
            {
                GithubApiEndpoint = new Uri("https://api.github.com"),
                NuGetSources = "https://api.nuget.org/v3/index.json",
                Mode = "organisation",
                GithubOrganisationName = "NuKeeperDotNet",
                AllowedChange = VersionChange.Major,
                LogLevel = LogLevel.Info
            };
        }
    }
}
