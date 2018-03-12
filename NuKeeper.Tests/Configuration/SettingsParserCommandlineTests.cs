using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Configuration;
using NuKeeper.Engine.Report;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Tests.Configuration
{
    [TestFixture]
    public class SettingsParserCommandlineTests
    {
        [Test]
        public void ValidRepoCommandLineIsParsed()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
        }

        [Test]
        public void ValidRepoCommandLineHasSpecifiedValues()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Repository));
            Assert.That(settings.ModalSettings.Repository, Is.Not.Null);
            Assert.That(settings.ModalSettings.Repository.RepositoryName, Is.EqualTo("NuKeeper"));
            Assert.That(settings.GithubAuthSettings.Token, Is.EqualTo("abc123"));
        }

        [Test]
        public void ValidRepoCommandLineHasDefaultGithubSettings()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.EqualTo(new Uri("https://api.github.com/")));
        }

        [Test]
        public void ValidRepoCommandLineHasDefaultUserSettings()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.NuGetSources, Is.EqualTo(new[] { "https://api.nuget.org/v3/index.json" }));
            Assert.That(settings.UserSettings.PackageIncludes, Is.Null);
            Assert.That(settings.UserSettings.PackageExcludes, Is.Null);
        }

        [Test]
        public void ValidRepoCommandLineHasDefaultUserOptions()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.MaxPullRequestsPerRepository, Is.EqualTo(3));
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.FromDays(7)));
        }

        [Test]
        public void ValidRepoCommandLineHasDefaultUserEnums()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.UserSettings.ForkMode, Is.EqualTo(ForkMode.PreferFork));
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
        }

        [Test]
        public void LogLevelOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("log=verbose");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Verbose));
        }

        [Test]
        public void MaxPrOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("maxpr=42");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.MaxPullRequestsPerRepository, Is.EqualTo(42));
        }

        [Test]
        public void AllowedChangeOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("change=patch");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.AllowedChange, Is.EqualTo(VersionChange.Patch));
        }

        [Test]
        public void ForkModeOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("fork=PreferSingleRepository");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.ForkMode, Is.EqualTo(ForkMode.PreferSingleRepository));
        }

        [Test]
        public void ReportModeOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("report=ReportOnly");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.ReportOnly));
        }

        [Test]
        public void InvalidModeIsNotParsed()
        {
            var commandLine = new List<string>
            {
                "mode=whatever",
                "repo=https://github.com/NuKeeperDotNet/NuKeeper",
                "t=abc123"
            };

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void MissingModeIsNotParsed()
        {
            var commandLine = new List<string>
            {
                "repo=https://github.com/NuKeeperDotNet/NuKeeper",
                "t=abc123"
            };

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void ShortOrgModeIsParsed()
        {
            var commandLine = new List<string>
            {
                "mode=org",
                "org=NuKeeperDotNet",
                "t=abc123"
            };

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Organisation));
        }

        [Test]
        public void ShortOrgModeinCapsIsParsed()
        {
            var commandLine = new List<string>
            {
                "mode=Org",
                "org=NuKeeperDotNet",
                "t=abc123"
            };

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Organisation));
        }

        [Test]
        public void ShortRepoModeIsParsed()
        {
            var commandLine = new List<string>
            {
                "mode=repo",
                "repo=https://github.com/NuKeeperDotNet/NuKeeper",
                "t=abc123"
            };

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Repository));
        }

        [Test]
        public void InvalidLogLevelIsNotParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("log=abigstick");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void InvalidChangeIsNotParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("change=fish");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void InvalidMaxPrIsNotParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("maxpr=surewhynot");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void ValidOrgCommandLineIsParsed()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings, Is.Not.Null);
            Assert.That(settings.ModalSettings.OrganisationName, Is.EqualTo("NuKeeperDotNet"));
        }

        [Test]
        public void ValidOrgCommandLineHasSpecifiedValues()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(GithubMode.Organisation));
            Assert.That(settings.ModalSettings.OrganisationName, Is.EqualTo("NuKeeperDotNet"));
            Assert.That(settings.GithubAuthSettings.Token, Is.EqualTo("abc123"));
        }

        [Test]
        public void ValidOrgCommandLineHasDefaultGithubValues()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.GithubAuthSettings.ApiBase, Is.EqualTo(new Uri("https://api.github.com/")));
        }

        [Test]
        public void ValidOrgCommandLineHasDefaultUserSettings()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.MaxPullRequestsPerRepository, Is.EqualTo(3));
            Assert.That(settings.UserSettings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.UserSettings.ForkMode, Is.EqualTo(ForkMode.PreferFork));
            Assert.That(settings.UserSettings.ReportMode, Is.EqualTo(ReportMode.Off));
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.FromDays(7)));
            Assert.That(settings.UserSettings.NuGetSources, Is.EqualTo(new[] { "https://api.nuget.org/v3/index.json" }));
        }

        [Test]
        public void MinPackageAgeIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("MinAge=3w");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.FromDays(21)));
        }

        [Test]
        public void InvalidMinPackageAgeIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("MinAge=78ff");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.Zero));
        }

        private static IEnumerable<string> ValidRepoCommandLine()
        {
            return new List<string>
            {
                "mode=repository",
                "repo=https://github.com/NuKeeperDotNet/NuKeeper",
                "t=abc123"
            };
        }

        private static IEnumerable<string> ValidOrgCommandLine()
        {
            return new List<string>
            {
                "mode=organisation",
                "org=NuKeeperDotNet",
                "t=abc123"
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

    static class StringAppend
    {
        public static IEnumerable<string> Append(this IEnumerable<string> values, string value)
        {
            return values.Concat(new[] {value});
        }
    }
}
