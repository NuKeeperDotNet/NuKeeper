using System;
using System.Linq;

namespace NuKeeper.Configuration
{
    public static class CommandLineParser
    {
        public static Settings ReadSettings(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please supply some command line arguments");
                return null;
            }

            var operationMode = args[0];

            Console.WriteLine($"Running NuKeeper in {operationMode} mode");

            switch (operationMode)
            {
                case Settings.RepositoryMode:
                    return new Settings(ReadSettingsForRepositoryMode(args));
                case Settings.OrganisationMode:
                    return new Settings(ReadSettingsForOrganisationMode(args));
                default:
                    Console.WriteLine($"Mode {operationMode} not supported");
                    return null;
            }
        }

        private static RepositoryModeSettings ReadSettingsForRepositoryMode(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments");
                return null;
            }

            var githubToken = args[1];
            var githubRepoUri = new Uri(args[2]);

            // general pattern is https://github.com/owner/reponame.git
            var githubHost = "https://api." + githubRepoUri.Host;
            var path = githubRepoUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositoryModeSettings
            {
                GithubUri = githubRepoUri,
                GithubToken = githubToken,
                GithubBaseUri = new Uri(githubHost),
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }

        private static OrganisationModeSettings ReadSettingsForOrganisationMode(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Not enough arguments");
                return null;
            }

            var githubToken = args[1];
            var githubHost = new Uri(args[2]);
            var githubOrganisationName = args[3];

            return new OrganisationModeSettings()
            {
                GithubBase = githubHost,
                GithubToken = githubToken,
                OrganisationName = githubOrganisationName
            };
        }
    }
}
