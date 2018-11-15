using System;
using System.Linq;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.AzureDevOps
{
    public class VstsSettingsReader : BaseSettingsReader
    {
        public override bool CanRead(Uri repositoryUri)
        {
            return repositoryUri?.Host.ContainsOrdinal("visualstudio.com") == true;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            // URL pattern is
            // https://{org}.visualstudio.com/{project}/_git/{repo}
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 3)
            {
                return null;
            }

            var org = repositoryUri.Host.Split('.')[0];
            var project = pathParts[0];
            var repoName = pathParts[2];

            return new RepositorySettings
            {
                ApiUri = new Uri($"https://dev.azure.com/{org}/"),
                RepositoryUri = new Uri($"https://dev.azure.com/{org}/{project}/_git/{repoName}/"),
                RepositoryName = repoName,
                RepositoryOwner = project
            };
        }
    }
}
