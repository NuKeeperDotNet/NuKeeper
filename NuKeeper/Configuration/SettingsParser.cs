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
        public static SettingsContainer ReadSettings(IEnumerable<string> args)
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

        public static SettingsContainer ParseToSettings(RawConfiguration settings)
        {
            var modalSettings = ReadModalSettings(settings);
            if (modalSettings == null)
            {
                return null;
            }

            var authSettings = new GithubAuthSettings(
                EnsureTrailingSlash(settings.GithubApiEndpoint),
                settings.GithubToken);

            var minPackageAge = DurationParser.Parse(settings.MinPackageAge);
            if (!minPackageAge.HasValue)
            {
                minPackageAge = TimeSpan.Zero;
                Console.WriteLine($"Min package age '{settings.MinPackageAge}' could not be parsed");
            }

            var userPrefs = new UserSettings
            {
                AllowedChange = settings.AllowedChange,
                ForkMode = settings.ForkMode,
                LogLevel = settings.LogLevel,
                ReportMode = settings.ReportMode,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository,
                NuGetSources = ReadNuGetSources(settings),
                PackageIncludes = ParseRegex(settings.Include, nameof(settings.Include)),
                PackageExcludes = ParseRegex(settings.Exclude, nameof(settings.Exclude)),
                MinimumPackageAge = minPackageAge.Value
            };

            return new SettingsContainer
            {
                ModalSettings = modalSettings,
                GithubAuthSettings = authSettings,
                UserSettings = userPrefs
            };
        }

        private static ModalSettings ReadModalSettings(RawConfiguration settings)
        {
            var mode = ParseMode(settings.Mode);

            if (!mode.HasValue)
            {
                Console.WriteLine($"Mode '{settings.Mode}' not supported");
                return null;
            }

            switch (mode.Value)
            {
                case GithubMode.Repository:
                    if (settings.GithubToken == null)
                    {
                        Console.WriteLine("Missing required github token");
                        return null;
                    }

                    if (settings.GithubRepositoryUri == null)
                    {
                        Console.WriteLine("Missing required repository uri");
                        return null;
                    }

                    return new ModalSettings
                    {
                        Mode = GithubMode.Repository,
                        Repository = ReadRepositorySettings(settings)
                    };

                case GithubMode.Organisation:
                    if (settings.GithubToken == null)
                    {
                        Console.WriteLine("Missing required github token");
                        return null;
                    }

                    if (string.IsNullOrWhiteSpace(settings.GithubOrganisationName))
                    {
                        Console.WriteLine("Missing required organisation name");
                        return null;
                    }
                    return new ModalSettings
                    {
                        Mode = GithubMode.Organisation,
                        OrganisationName = settings.GithubOrganisationName
                    };

                case GithubMode.Inspect:
                {
                        return new ModalSettings
                        {
                            Mode = GithubMode.Inspect
                        };
                }

                default:
                    Console.WriteLine($"Mode parse went wrong: {settings.Mode}");
                    return null;
            }
        }

        private static GithubMode? ParseMode(string mode)
        {
            var modeString = mode?.ToLowerInvariant() ?? string.Empty;

            switch (modeString)
            {
                case ModeNames.Repo:
                case ModeNames.Repository:
                    return  GithubMode.Repository;

                case ModeNames.Org:
                case ModeNames.Organisation:
                    return GithubMode.Organisation;

                case ModeNames.Inspect:
                    return GithubMode.Inspect;

                default:
                    return null;
            }
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

        private static RepositorySettings ReadRepositorySettings(RawConfiguration settings)
        {
            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = settings.GithubRepositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
                {
                    GithubUri = settings.GithubRepositoryUri,
                    RepositoryName = repoName,
                    RepositoryOwner = repoOwner
            };
        }

        private static string[] ReadNuGetSources(RawConfiguration settings)
        {
            return settings.NuGetSources.Split(new []{';'}, StringSplitOptions.RemoveEmptyEntries);
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
