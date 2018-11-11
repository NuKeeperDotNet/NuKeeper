using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.AzureDevOps
{
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

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.SingleRepositoryOnly;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable("NuKeeper_azure_devops_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            // URL pattern is
            // https://dev.azure.com/{org}/{project}/_git/{repo}/
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
