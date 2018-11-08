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
        public AuthSettings AuthSettings(Uri uri, string accessToken)
        {
            var testUri = uri ?? new Uri("https://api.github.com/");

            testUri = UriFormats.EnsureTrailingSlash(testUri);

            if (!testUri.IsWellFormedOriginalString()
                || (testUri.Scheme != "http" && testUri.Scheme != "https"))
            {
                return null;
            }

            if (testUri.Host == "github.com")
            {
                testUri = new Uri("https://api.github.com/");
            }

            var envToken = Environment.GetEnvironmentVariable("NuKeeper_github_token");
            var token = Concat.FirstValue(envToken, accessToken);

            return new AuthSettings(testUri, token);
        }

        public Platform Platform => Platform.GitHub;

        public bool CanRead(Uri repositoryUri)
        {
            return repositoryUri.Host.Contains("github");
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            Uri apiUrl;
            if (repositoryUri.Host == "github.com")
            {
                apiUrl = new Uri("https://api.github.com/");
            }
            else
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
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }
    }
}
