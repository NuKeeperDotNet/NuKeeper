using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.AzureDevOps
{
    public class VstsSettingsReader : BaseSettingsReader
    {
        public const string PlatformHost = "visualstudio.com";
        
        private readonly IGitDiscoveryDriver _gitDriver;
        private bool _isFromLocalGitRepo;
        
        public VstsSettingsReader(IGitDiscoveryDriver gitDriver)
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
                repositoryUri = repositoryUri.GetRemoteUriFromLocalRepo(_gitDriver, PlatformHost);
                _isFromLocalGitRepo = true;
            }
       
            return repositoryUri?.Host.ContainsOrdinal(PlatformHost) == true;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            return _isFromLocalGitRepo ? CreateSettingsFromLocal(repositoryUri) : CreateSettingsFromRemote(repositoryUri);
        }

        private RepositorySettings CreateSettingsFromRemote(Uri repositoryUri)
        {
            // URL pattern is
            // https://{org}.visualstudio.com/{project}/_git/{repo} or
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var org = repositoryUri.Host.Split('.')[0];
            string repoName, project;

            if (pathParts.Count == 3)
            {
                project = pathParts[0];
                repoName = pathParts[2];
            }
            else
            {
                throw new NuKeeperException("Unknown format. Format should be https://{org}.visualstudio.com/{project}/_git/{repo}");
            }

            return RepositorySettings(org, project, repoName);
        }


        private RepositorySettings CreateSettingsFromLocal(Uri repositoryUri)
        {
            var remoteInfo = new RemoteInfo();

            var localFolder = repositoryUri;
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
                    remoteInfo.RemoteName = "origin";
                    remoteInfo.WorkingFolder = localFolder;
                }
            }
            else
            {
                throw new NuKeeperException("No git repository found");
            }

            // URL pattern is
            // https://{org}.visualstudio.com/{project}/_git/{repo} or
            // https://{org}.visualstudio.com/_git/{repo} for the default repo
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var org = repositoryUri.Host.Split('.')[0];
            string repoName, project;

            if (pathParts.Count == 3)
            {
                project = pathParts[0];
                repoName = pathParts[2];
            }
            else if (pathParts.Count == 4)
            {
                project = pathParts[1];
                repoName = pathParts[3];
            }
            else if (pathParts.Count == 2)
            {
                project = pathParts[1];
                repoName = pathParts[1];
            }
            else
            {
                throw new NuKeeperException("Unknown local format. Format should be https://{org}.visualstudio.com/_git/{repo}");
            }

            return RepositorySettings(org, project, repoName, remoteInfo);
        }
        
        private RepositorySettings RepositorySettings (string org, string project, string repoName, RemoteInfo remoteInfo = null) => new RepositorySettings
        {
            ApiUri = new Uri($"https://{org}.visualstudio.com/"),
            RepositoryUri = new Uri($"https://{org}.visualstudio.com/{project}/_git/{repoName}/"),
            RepositoryName = repoName,
            RepositoryOwner = project,
            RemoteInfo = remoteInfo
        };
    }
}
