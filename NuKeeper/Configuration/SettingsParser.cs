using System;
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
                    return new Settings(ReadSettingsForRepositoryMode(settings));
                case Settings.OrganisationMode:
                    return new Settings(ReadSettingsForOrganisationMode(settings));
                default:
                    Console.WriteLine($"Mode {settings.Mode} not supported");
                    return null;
            }
        }

        private static RepositoryModeSettings ReadSettingsForRepositoryMode(RawConfiguration settings)
        {
            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = settings.GithubRepositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositoryModeSettings
            {
                GithubUri = settings.GithubRepositoryUri,
                GithubToken = settings.GithubToken,
                GithubApiBase = EnsureTrailingSlash(settings.GithubApiEndpoint),
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            };
        }

        private static OrganisationModeSettings ReadSettingsForOrganisationMode(RawConfiguration settings)
        {
            var githubToken = settings.GithubToken;
            var githubHost = settings.GithubApiEndpoint;
            var githubOrganisationName = settings.GithubOrganisationName;

            return new OrganisationModeSettings
            {
                GithubApiBase = EnsureTrailingSlash(githubHost),
                GithubToken = githubToken,
                OrganisationName = githubOrganisationName,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            };
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
