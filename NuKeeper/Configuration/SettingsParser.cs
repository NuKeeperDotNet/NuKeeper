﻿using System;
using System.Linq;
using EasyConfig;
using EasyConfig.Exceptions;

namespace NuKeeper.Configuration
{
    public static class SettingsParser
    {
        public static Settings ReadSettings(string[] args)
        {
            RawConfiguration settings;
            try
            {
                 Config.UseJson("config.json");
                 settings = Config.Populate<RawConfiguration>(args);
            }
            catch(EasyConfigException e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            
            Console.WriteLine($"Running NuKeeper in {settings.Mode} mode");

            switch (settings.Mode)
            {
                case Settings.RepositoryMode:
                    return ReadSettingsForRepositoryMode(settings);
                case Settings.OrganisationMode:
                    return ReadSettingsForOrganisationMode(settings);
                default:
                    Console.WriteLine($"Mode {settings.Mode} not supported");
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

            return new Settings(new RepositoryModeSettings
            {
                GithubUri = settings.GithubRepositoryUri,
                GithubToken = settings.GithubToken,
                GithubApiBase = EnsureTrailingSlash(settings.GithubApiEndpoint),
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            });
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

            return new Settings(new OrganisationModeSettings
            {
                GithubApiBase = EnsureTrailingSlash(githubHost),
                GithubToken = githubToken,
                OrganisationName = githubOrganisationName,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            });
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
