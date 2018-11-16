using System;
using System.Linq;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsSettingsReader : BaseSettingsReader
    {
        private readonly IGitDiscoveryDriver _gitDriver;

        public AzureDevOpsSettingsReader(IGitDiscoveryDriver gitDriver)
        {
            _gitDriver = gitDriver;
        }

        public override bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return false;
            }

            // Is the specified folder already a git repository?
            if (repositoryUri.IsFile)
            {
                if (_gitDriver.IsGitRepo(repositoryUri))
                {
                    // Check the origin remotes
                    var remotes = _gitDriver.GetRemotes(repositoryUri);
                    var origin = remotes.FirstOrDefault(a =>
                        a.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                    if (origin != null)
                        repositoryUri = origin.Url;
                }

            }

            // Did we specify a Azure DevOps?
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


            Uri localRepositoryUri =null;
            Uri workingFolder = repositoryUri;
            string branchName = string.Empty;
            string remoteName = string.Empty;

            if (_gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var remotes = _gitDriver.GetRemotes(repositoryUri);
                var origin = remotes.FirstOrDefault(a =>
                    a.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                if (origin != null)
                {
                    localRepositoryUri = _gitDriver.DiscoverRepo(repositoryUri); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    branchName = _gitDriver.GetCurrentHead(localRepositoryUri);
                    remoteName = origin.Name;
                }

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
                RepositoryOwner = project,
                LocalRepositoryUri = localRepositoryUri,
                WorkingFolder = workingFolder,
                BranchName = branchName,
                RemoteName = remoteName
            };
        }
    }
}
