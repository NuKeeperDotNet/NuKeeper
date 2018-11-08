using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.AzureDevOps
{
    // URL pattern is
    // https://dev.azure.com/{org}/{project}/_git/{repo}/
    public class AzureDevOpsSettingsReader : ISettingsReader
    {
        public Platform Platform => Platform.AzureDevOps;

        public bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null || repositoryUri.Host != "dev.azure.com")
            {
                return false;
            }

            return true;
        }

        public AuthSettings AuthSettings(Uri apiUri, string accessToken)
        {

            if (apiUri == null)
            {
                return null;
            }

            var testUri = UriFormats.EnsureTrailingSlash(apiUri);

            if (!testUri.IsWellFormedOriginalString()
                || (testUri.Scheme != "http" && testUri.Scheme != "https"))
            {
                return null;
            }

            var envToken = Environment.GetEnvironmentVariable("NuKeeper_azure_devops_token");
            var token = Concat.FirstValue(envToken, accessToken);

            return new AuthSettings(testUri, token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 4)
            {
                return null;
            }

            var org = pathParts[0];
            var project = pathParts[1];
            var repoName = pathParts[3];

            return new RepositorySettings
            {
                ApiUri = new Uri($"https://dev.azure.com/{org}/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = project
            };
        }
    }
}
