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
        private const string PlatformHost = "dev.azure.com";
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
                repositoryUri = repositoryUri.GetRemoteUriFromLocalRepo(_gitDriver, PlatformHost);
            }

            // Did we specify a Azure DevOps url?
            return repositoryUri?.Host.ContainsOrdinal(PlatformHost) == true;
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
            var remoteInfo = new RemoteInfo();

            var localCopy = repositoryUri;
            if (_gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var origin = _gitDriver.GetRemoteForPlatform(repositoryUri, PlatformHost);

                if (origin != null)
                {
                    remoteInfo.LocalRepositoryUri = _gitDriver.DiscoverRepo(localCopy); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    remoteInfo.WorkingFolder = localCopy;
                    remoteInfo.BranchName = _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
                    remoteInfo.RemoteName = origin.Name;
                }
            }
            else
            {
                throw new NuKeeperException("No git repository found");
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
