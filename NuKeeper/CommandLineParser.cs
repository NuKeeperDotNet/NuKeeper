using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper
{
    public static class CommandLineParser
    {
        public const string RepositoryMode = "repository";
        public const string OrganisationMode = "organisation";

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
                case RepositoryMode:
                    return ReadSettingsForRepositoryMode(args);
                case OrganisationMode:
                    return ReadSettingsForOrganisationMode(args);
                default:
                    throw new ArgumentException($"Mode {operationMode} not supported");
            }
        }

        private static Settings ReadSettingsForRepositoryMode(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Not enough arguments");
                return null;
            }

            var gitRepoUri = new Uri(args[1]);
            var gitToken = args[2];

            // general pattern is https://github.com/owner/reponame.git
            var gitHost = "https://api." + gitRepoUri.Host;
            var path = gitRepoUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new Settings
            {
                GitUri = gitRepoUri,
                GithubToken = gitToken,
                GithubBaseUri = new Uri(gitHost),
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }

        private static Settings ReadSettingsForOrganisationMode(string[] arg)
        {
            throw new NotImplementedException();
        }
    }
}
