using System;
using System.Linq;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsSettingsReader : ISettingsReader
    {
        public Platform Platform => Platform.AzureDevOps;

        public bool CanRead(Uri repositoryUri)
        {
            // general pattern is either of
            // https://dev.azure.com/{org}/{project}/_git/{repo}/
            if (repositoryUri == null || repositoryUri.Host != "dev.azure.com")
            {
                return false;
            }
            return true;
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

            if (pathParts.Count != 4)
            {
                return null;
            }

            var project = pathParts[1];
            var repoName = pathParts[3];

            return new RepositorySettings
            {
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = project
            };
        }

        public AuthSettings AuthSettings(Uri apiEndpoint, string accessToken)
        {
            var path = apiEndpoint.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count <= 1)
            {
                return null;
            }

            var org = pathParts[0];

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

            return new AuthSettings(new Uri($"https://dev.azure.com/{org}/"), token);
        }
    }
}
