using System;
using System.Linq;
using NuKeeper.Nuget.Api;

namespace NuKeeper
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Supply a git url");
                return;
            }
            var settings = ReadSettings(args);

            var lookups = new PackageUpdatesLookup(new ApiPackageLookup());
            var engine = new Engine(lookups, settings);
            engine.Run()
                .GetAwaiter().GetResult();
        }

        private static Settings ReadSettings(string[] args)
        {
            var gitRepoUri = new Uri(args[0]);
            var gitToken = args[1];

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
    }
}
