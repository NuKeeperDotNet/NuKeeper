using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsSettingsReader : BaseSettingsReader
    {
        private readonly IGitDiscoveryDriver _gitDriver;
        private bool _isLocalGitRepo;
        
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
                _isLocalGitRepo = true;
                repositoryUri.SetUriFromLocalRepo(_gitDriver);
            }

            // Did we specify a Azure DevOps?
            return repositoryUri?.Host.ContainsOrdinal("dev.azure.com") == true;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            return _isLocalGitRepo ? CreateSettingsFromLocal(repositoryUri) : CreateSettingsFromRemote(repositoryUri);
        }

        private RepositorySettings CreateSettingsFromRemote(Uri repositoryUri)
        {
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

            return CreateRepositorySettings(org, repositoryUri, project, repoName);
        }
        
        private RepositorySettings CreateSettingsFromLocal(Uri repositoryUri)
        {
            RemoteInfo remoteInfo = new RemoteInfo();
            
            if (_gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var remotes = _gitDriver.GetRemotes(repositoryUri);
                var origin = remotes.FirstOrDefault(a =>
                    a.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                if (origin != null)
                {
                    remoteInfo.LocalRepositoryUri = _gitDriver.DiscoverRepo(repositoryUri); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    remoteInfo.BranchName = _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
                    remoteInfo.RemoteName = origin.Name;
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

            return CreateRepositorySettings(org, repositoryUri, project, repoName, remoteInfo);
        }
        
        private RepositorySettings CreateRepositorySettings (string org, Uri repositoryUri, string project, string repoName, RemoteInfo remoteInfo = null) => new RepositorySettings
        {
            ApiUri = new Uri($"https://dev.azure.com/{org}/"),
            RepositoryUri = repositoryUri,
            RepositoryName = repoName,
            RepositoryOwner = project,
            RemoteInfo = remoteInfo
        };
    }
}
