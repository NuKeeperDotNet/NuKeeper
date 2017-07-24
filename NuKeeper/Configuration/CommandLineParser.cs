using System;
using System.Linq;
using EasyConfig;
using EasyConfig.Exceptions;

namespace NuKeeper.Configuration
{
    public static class CommandLineParser
    {
        public static Settings ReadSettings(string[] args)
        {
            CommandLineArguments settings;
            try
            {
                 settings = Config.Populate<CommandLineArguments>(args);
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

        private static RepositoryModeSettings ReadSettingsForRepositoryMode(CommandLineArguments settings)
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
                GithubUser = settings.GithubUser,
                GithubToken = settings.GithubToken,
                GithubApiBase = settings.GithubApiEndpoint,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            };
        }

        private static OrganisationModeSettings ReadSettingsForOrganisationMode(CommandLineArguments settings)
        {
            var githubUser = settings.GithubUser;
            var githubToken = settings.GithubToken;
            var githubHost = settings.GithubApiEndpoint;
            var githubOrganisationName = settings.GithubOrganisationName;

            return new OrganisationModeSettings
            {
                GithubApiBase = githubHost,
                GithubUser = githubUser,
                GithubToken = githubToken,
                OrganisationName = githubOrganisationName,
                MaxPullRequestsPerRepository = settings.MaxPullRequestsPerRepository
            };
        }
    }
}
