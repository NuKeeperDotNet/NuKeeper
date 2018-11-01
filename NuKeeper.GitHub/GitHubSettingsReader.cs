using System;
using System.Linq;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.GitHub
{
    public static class GitHubSettingsReader
    {
        public static RepositorySettings ReadRepositorySettings(Uri gitHubRepositoryUri)
        {
            if (gitHubRepositoryUri == null)
            {
                return null;
            }

            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = gitHubRepositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count < 2)
            {
                return null;
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
                {
                    Uri = gitHubRepositoryUri,
                    RepositoryName = repoName,
                    RepositoryOwner = repoOwner
            };
        }
    }
}
