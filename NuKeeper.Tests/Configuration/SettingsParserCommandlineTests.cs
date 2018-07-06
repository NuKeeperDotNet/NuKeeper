using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NUnit.Framework;

namespace NuKeeper.Tests.Configuration
{
    [TestFixture]
    public class SettingsParserCommandlineTests
    {
        [Test]
        public void EmptyListIsParsedAsInspect()
        {
            var commandLine = new List<string>();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
        }

        [Test]
        public void ValidRepoCommandLineIsParsed()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
        }

        [Test]
        public void RepoCommandLineWithoutGithubTokenIsNotParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Where(i => ! i.StartsWith("t="));

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void ValidRepoCommandLineHasSpecifiedValues()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Repository));
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
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
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
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(10));
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
        public void MaxRepoOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("maxrepo=51");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.UserSettings.MaxRepositoriesChanged, Is.EqualTo(51));
        }

        [Test]
        public void SourcesDefaultIsParsed()
        {
            var commandLine = ValidRepoCommandLine();

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            var sources = settings.UserSettings.NuGetSources;
            Assert.That(sources, Is.Null);
        }

        [Test]
        public void SourcesOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("sources=http://foo;file://blah");

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            var sources = settings.UserSettings.NuGetSources;
            Assert.That(sources.Items.Count, Is.EqualTo(2));
            Assert.That(sources.Items.First(), Is.EqualTo("http://foo/"));
            Assert.That(sources.Items.Skip(1).First(), Is.EqualTo("file://blah/"));
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
        public void MissingModeIsParsedAsInspect()
        {
            var commandLine = new List<string>
            {
                "log=verbose"
            };

            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
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
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Organisation));
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
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Organisation));
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
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Repository));
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
        public void OrgCommandLineWithoutGithubTokenIsNotParsed()
        {
            var commandLine = ValidOrgCommandLine()
                .Where(i => !i.StartsWith("t="));

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Null);
        }

        [Test]
        public void ValidOrgCommandLineHasSpecifiedValues()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            AssertSettingsNotNull(settings);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Organisation));
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
            Assert.That(settings.UserSettings.NuGetSources, Is.Null);
        }

        [Test]
        public void ValidMinPackageAgeIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("age=3w");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.FromDays(21)));
        }

        [Test]
        public void ZeroMinPackageAgeIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("age=0");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void InvalidMinPackageAgeIsParsedAsZero()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("age=78ff");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.UserSettings, Is.Not.Null);
            Assert.That(settings.UserSettings.MinimumPackageAge, Is.EqualTo(TimeSpan.Zero));
        }

        [Test]
        public void ValidInspectCommandLineIsParsed()
        {
            var commandLine = ValidInspectCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings, Is.Not.Null);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
        }

        [Test]
        public void InspectCommandLineWithDirIsParsed()
        {
            var commandLine = ValidInspectCommandLine()
                .Append("dir=/foo/bar");
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings, Is.Not.Null);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Inspect));
            Assert.That(settings.UserSettings.Directory, Is.EqualTo("/foo/bar"));
        }

        [Test]
        public void ValidUpdateCommandLineIsParsed()
        {
            var commandLine = ValidUpdateCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings, Is.Not.Null);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Update));
        }

        [Test]
        public void UpdateCommandLineWithDirIsParsed()
        {
            var commandLine = ValidUpdateCommandLine()
                .Append("dir=/foo/bar");
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.ModalSettings, Is.Not.Null);
            Assert.That(settings.ModalSettings.Mode, Is.EqualTo(RunMode.Update));
            Assert.That(settings.UserSettings.Directory, Is.EqualTo("/foo/bar"));
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

        private static IEnumerable<string> ValidInspectCommandLine()
        {
            return new List<string>
            {
                "mode=inspect"
            };
        }

        private static IEnumerable<string> ValidUpdateCommandLine()
        {
            return new List<string>
            {
                "mode=update"
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
