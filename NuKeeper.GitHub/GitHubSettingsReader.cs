using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using System;
using System.Linq;

namespace NuKeeper.GitHub
{
    public class GitHubSettingsReader : ISettingsReader
    {
        public Platform Platform => Platform.GitHub;

        public bool CanRead(Uri repositoryUri)
        {
            return repositoryUri.Host.Contains("github");
        }

        public AuthSettings AuthSettings(Uri apiUri, string accessToken)
        {
            if (apiUri == null)
            {
                return null;
            }

            apiUri = UriFormats.EnsureTrailingSlash(apiUri);

            if (!apiUri.IsWellFormedOriginalString()
                || (apiUri.Scheme != "http" && apiUri.Scheme != "https"))
            {
                return null;
            }

            var envToken = Environment.GetEnvironmentVariable("NuKeeper_github_token");
            var token = Concat.FirstValue(envToken, accessToken);

            return new AuthSettings(apiUri, token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                return null;
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
            {
                ApiUri = new Uri("https://api.github.com/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }
    }
}
