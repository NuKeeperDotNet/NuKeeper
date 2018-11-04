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
        private readonly FileSettings _fileSettings;

        public GitHubSettingsReader(IFileSettingsCache settingsCache)
        {
            _fileSettings = settingsCache.GetSettings();
        }

        public AuthSettings AuthSettings(string apiEndpoint, string accessToken)
        {
            const string defaultGithubApi = "https://api.github.com/";
            var api = Concat.FirstValue(apiEndpoint, _fileSettings.Api, defaultGithubApi);

            var baseUri = new Uri(api, UriKind.Absolute);
            baseUri = UriFormats.EnsureTrailingSlash(baseUri);

            if (
                !baseUri.IsWellFormedOriginalString()
                || baseUri.AbsolutePath != "/"
                || (baseUri.Scheme != "http" && baseUri.Scheme != "https")
            )
            {
                return null;
            }

            var envToken = Environment.GetEnvironmentVariable("NuKeeper_github_token");
            var token = Concat.FirstValue(envToken, accessToken);

            return new AuthSettings(baseUri, token);
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
                Uri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }
    }
}
