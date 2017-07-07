using System;
using System.Linq;

namespace NuKeeper.Configuration
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
                    return new Settings(ReadSettingsForRepositoryMode(args));
                case OrganisationMode:
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

            return new RepositoryModeSettings
            {
                GitUri = gitRepoUri,
                GithubToken = gitToken,
                GithubBaseUri = new Uri(gitHost),
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }

        private static OrganisationModeSettings ReadSettingsForOrganisationMode(string[] arg)
        {
            throw new NotImplementedException();
        }
    }
}
