using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using EasyConfig;
using EasyConfig.Exceptions;

namespace NuKeeper.Configuration
{
    public static class SettingsParser
    {
        public static Settings ReadSettings(IEnumerable<string> args)
        {
            var rawSettings = ParseToRaw(args);
            if (rawSettings == null)
            {
                return null;
            }
            
            Console.WriteLine($"Running NuKeeper in {rawSettings.Mode} mode");

            return ParseToSettings(rawSettings);
        }

        private static RawConfiguration ParseToRaw(IEnumerable<string> args)
        {
            try
            {
                Config.UseJson("config.json");
                return Config.Populate<RawConfiguration>(args.ToArray());
            }
            catch (EasyConfigException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
        }

        public static Settings ParseToSettings(RawConfiguration settings)
        {
            Settings result;

            switch (settings.Mode)
            {
                case Settings.RepositoryMode:
                    result = ReadSettingsForRepositoryMode(settings);
                    break;

                case Settings.OrganisationMode:
                    result = ReadSettingsForOrganisationMode(settings);
                    break;

                default:
                    Console.WriteLine($"Mode {settings.Mode} not supported");
                    return null;
            }

            result.LogLevel = settings.LogLevel;
            result.AllowedChange = settings.AllowedChange;

            result.NuGetSources = ReadNuGetSources(settings);
            result.PackageIncludes = ParseRegex(settings.Include, nameof(settings.Include));
            result.PackageExcludes = ParseRegex(settings.Exclude, nameof(settings.Exclude));
            return result;
        }

        private static Regex ParseRegex(string regex, string optionName)
        {
            if (string.IsNullOrWhiteSpace(regex))
            {
                return null;
            }

            try
            {
                return new Regex(regex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to parse {regex} for option {optionName} : {ex.Message}");
                return null;
            }
        }

        private static Settings ReadSettingsForRepositoryMode(RawConfiguration settings)
        {
            if (settings.GithubRepositoryUri == null)
            {
                Console.WriteLine("Missing required repository uri");
                return null;
            }

            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = settings.GithubRepositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            var repoSettings = new RepositoryModeSettings
                {
                    GithubUri = settings.GithubRepositoryUri,
                    GithubToken = settings.GithubToken,
                    GithubApiBase = EnsureTrailingSlash(settings.GithubApiEndpoint),
                    RepositoryName = repoName,
                    RepositoryOwner = repoOwner,
                    MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            };

            return new Settings(repoSettings);
        }

        private static string[] ReadNuGetSources(RawConfiguration settings)
        {
            return settings.NuGetSources.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
        }

        private static Settings ReadSettingsForOrganisationMode(RawConfiguration settings)
        {
            if (string.IsNullOrWhiteSpace(settings.GithubOrganisationName))
            {
                Console.WriteLine("Missing required organisation name");
                return null;
            }

            var githubToken = settings.GithubToken;
            var githubHost = settings.GithubApiEndpoint;
            var githubOrganisationName = settings.GithubOrganisationName;

            var orgSettings = new OrganisationModeSettings
                {
                    GithubApiBase = EnsureTrailingSlash(githubHost),
                    GithubToken = githubToken,
                    OrganisationName = githubOrganisationName,
                    MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            };

            return new Settings(orgSettings);
        }

        private static Uri EnsureTrailingSlash(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            var path = uri.ToString();

            if (path.EndsWith("/"))
            {
                return uri;
            }

            return new Uri(path + "/");
        }
    }
}
