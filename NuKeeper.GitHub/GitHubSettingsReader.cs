using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using System;
using System.Linq;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.GitHub
{
    public class GitHubSettingsReader : ISettingsReader
    {
        private const string UrlPattern = "https://github.com/{owner}/{reponame}.git";

        public Platform Platform => Platform.GitHub;

        public bool CanRead(Uri repositoryUri)
        {
            return repositoryUri?.Host.Contains("github", StringComparison.OrdinalIgnoreCase) == true;
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.PreferFork;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable("NuKeeper_github_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch = null)
        {
            if (repositoryUri == null)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided null and format should be {UrlPattern}");
            }

            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri.ToString()} and format should be {UrlPattern}");
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
            {
                ApiUri = new Uri("https://api.github.com/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                TargetBranch = targetBranch
            };
        }
    }
}
