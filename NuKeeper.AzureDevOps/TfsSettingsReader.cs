using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.AzureDevOps
{
    public class TfsSettingsReader : BaseSettingsReader
    {
        private const string PlatformHost = "tfs";
        private readonly IGitDiscoveryDriver _gitDriver;
        private bool _isFromLocalGitRepo;

        public TfsSettingsReader(IGitDiscoveryDriver gitDriver)
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

            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var tfsInPath = pathParts.Count > 0 && pathParts[0].ContainsOrdinal(PlatformHost);
            var tfsInHost = repositoryUri.Host.ContainsOrdinal(PlatformHost);
            return tfsInPath || tfsInHost;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            return _isFromLocalGitRepo ? CreateSettingsFromLocal(repositoryUri) : CreateSettingsFromRemote(repositoryUri);
        }

        private static RepositorySettings CreateSettingsFromRemote(Uri repositoryUri)
        {
            return RepositorySettings(repositoryUri);
        }


        private RepositorySettings CreateSettingsFromLocal(Uri repositoryUri)
        {
            var remoteInfo = new RemoteInfo();

            var localFolder = repositoryUri;
            if (_gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var origin = _gitDriver.GetRemoteForPlatform(repositoryUri, PlatformHost);

                if (origin != null)
                {
                    remoteInfo.LocalRepositoryUri = _gitDriver.DiscoverRepo(repositoryUri); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    remoteInfo.BranchName = _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
                    remoteInfo.RemoteName = origin.Name;
                    remoteInfo.WorkingFolder = localFolder;
                }
            }
            else
            {
                throw new NuKeeperException("No git repository found");
            }

            return RepositorySettings(repositoryUri, remoteInfo);
        }

        private static RepositorySettings RepositorySettings(Uri repositoryUri, RemoteInfo remoteInfo = null)
        {
            // URL pattern is
            // https://tfs.company.local:{port}/<nothingOrVirtualSite>/{project}/_git/{repo}
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var gitLocation = pathParts.IndexOf("_git");
            if (gitLocation == -1)
            {
                throw new NuKeeperException("Unknown format. Format should be http(s)://tfs.company.local:port/<nothingOrVirtualSite>/{project}/_git/{repo}");
            }

            var project = pathParts[gitLocation - 1];
            var repoName = pathParts[gitLocation + 1];
            var apiPathParts = pathParts.Take(gitLocation - 1).ToArray();

            return new RepositorySettings
            {
                ApiUri = new Uri($"{repositoryUri.Scheme}://{repositoryUri.Host}:{repositoryUri.Port}/{apiPathParts.JoinWithSeparator("/")}"),
                RepositoryUri = new Uri(
                    $"{repositoryUri.Scheme}://user:--PasswordToReplace--@{repositoryUri.Host}:{repositoryUri.Port}/{apiPathParts.JoinWithSeparator("/")}/{project}/_git/{repoName}/"),
                RepositoryName = repoName,
                RepositoryOwner = project,
                RemoteInfo = remoteInfo
            };
        }
    }
}
