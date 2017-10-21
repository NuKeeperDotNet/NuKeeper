using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Configuration;
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

            Assert.That(settings, Is.Not.Null);
        }

        [Test]
        public void ValidRepoCommandLineHasSpecifiedValues()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings.Mode, Is.EqualTo("repository"));
            Assert.That(settings.Repository, Is.Not.Null);
            Assert.That(settings.Repository.RepositoryName, Is.EqualTo("NuKeeper"));
            Assert.That(settings.GithubToken, Is.EqualTo("abc123"));
        }

        [Test]
        public void ValidRepoCommandLineHasDefaultValues()
        {
            var commandLine = ValidRepoCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings.MaxPullRequestsPerRepository, Is.EqualTo(3));
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.GithubApiBase, Is.EqualTo(new Uri("https://api.github.com/")));
            Assert.That(settings.NuGetSources, Is.EqualTo(new [] {"https://api.nuget.org/v3/index.json"}));
        }

        [Test]
        public void LogLevelOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("log=verbose");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.Verbose));
        }

        [Test]
        public void MaxPrOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("maxpr=42");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.MaxPullRequestsPerRepository, Is.EqualTo(42));
        }

        [Test]
        public void AllowedChangeOverrideIsParsed()
        {
            var commandLine = ValidRepoCommandLine()
                .Append("change=patch");

            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.AllowedChange, Is.EqualTo(VersionChange.Patch));
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
        public void ValidOrgCommandLineIsParsed()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings, Is.Not.Null);
            Assert.That(settings.Organisation, Is.Not.Null);
            Assert.That(settings.Organisation.OrganisationName, Is.EqualTo("NuKeeperDotNet"));
        }

        [Test]
        public void ValidOrgCommandLineHasSpecifiedValues()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings.Mode, Is.EqualTo("organisation"));
            Assert.That(settings.Organisation, Is.Not.Null);
            Assert.That(settings.Organisation.OrganisationName, Is.EqualTo("NuKeeperDotNet"));
            Assert.That(settings.GithubToken, Is.EqualTo("abc123"));
        }

        [Test]
        public void ValidOrgCommandLineHasDefaultValues()
        {
            var commandLine = ValidOrgCommandLine();
            var settings = SettingsParser.ReadSettings(commandLine);

            Assert.That(settings.MaxPullRequestsPerRepository, Is.EqualTo(3));
            Assert.That(settings.LogLevel, Is.EqualTo(LogLevel.Info));
            Assert.That(settings.GithubApiBase, Is.EqualTo(new Uri("https://api.github.com/")));
            Assert.That(settings.NuGetSources, Is.EqualTo(new[] { "https://api.nuget.org/v3/index.json" }));
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
    }

    static class StringAppend
    {
        public static IEnumerable<string> Append(this IEnumerable<string> values, string value)
        {
            return values.Concat(new[] {value});
        }
    }
}
