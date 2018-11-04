using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsSettingsReader : ISettingsReader
    {
        private readonly FileSettings _fileSettings;

        public AzureDevOpsSettingsReader(IFileSettingsCache settingsCache)
        {
            _fileSettings = settingsCache.GetSettings();
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            // general pattern is either of
            // https://{org}.visualstudio.com/{project}/_git/{repo}/
            // https://dev.azure.com/{org}/{project}/_git/{repo}/
            // from this we extract owner and repo name
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count < 3)
            {
                return null;
            }

            var project = pathParts[0];
            var repoName = pathParts[2];

            return new RepositorySettings
            {
                Uri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = project
            };
        }

        public AuthSettings AuthSettings(string apiEndpoint, string accessToken)
        {
            const string defaultGithubApi = "https://dev.azure.com/";
            var api = Concat.FirstValue(apiEndpoint, _fileSettings.Api, defaultGithubApi);
            if (!Uri.TryCreate(api, UriKind.Absolute, out var baseUri))
            {
                baseUri = null;
            }

            baseUri = UriFormats.EnsureTrailingSlash(baseUri);
            var token = "";
            if (!string.IsNullOrWhiteSpace(accessToken))
            {
                token = accessToken;
            }

            var envToken = Environment.GetEnvironmentVariable("NuKeeper_azure_devops_token");
            if (!string.IsNullOrWhiteSpace(envToken))
            {
                token = envToken;
            }

            return new AuthSettings(baseUri, token);
        }
    }
}
