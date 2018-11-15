using System;
using System.Linq;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsSettingsReader : BaseSettingsReader
    {
        public override bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null || repositoryUri.Host != "dev.azure.com")
            {
                return false;
            }

            return true;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri)
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
