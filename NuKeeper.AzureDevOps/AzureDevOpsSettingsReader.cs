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
        private const string UrlPattern = "https://dev.azure.com/{org}/{project}/_git/{repo}/";

        private readonly IGitDiscoveryDriver _gitDriver;
        private bool _isLocalGitRepo;

        public AzureDevOpsSettingsReader(IGitDiscoveryDriver gitDriver, IEnvironmentVariablesProvider environmentVariablesProvider)
            : base(environmentVariablesProvider)
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
            return repositoryUri?.Host.Contains(PlatformHost, StringComparison.OrdinalIgnoreCase) == true;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            var settings = _isLocalGitRepo ? CreateSettingsFromLocal(repositoryUri, targetBranch) : CreateSettingsFromRemote(repositoryUri);
            if (settings == null)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri.ToString()} and format should be {UrlPattern}");
            }

            return settings;
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
            var project = Uri.UnescapeDataString(pathParts[1]);
            var repoName = Uri.UnescapeDataString(pathParts[3]);

            return CreateRepositorySettings(org, repositoryUri, project, repoName);
        }

        private RepositorySettings CreateSettingsFromLocal(Uri repositoryUri, string targetBranch)
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
                    remoteInfo.BranchName = targetBranch ?? _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
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
            var project = Uri.UnescapeDataString(pathParts[1]);
            var repoName = Uri.UnescapeDataString(pathParts[3]);

            return CreateRepositorySettings(org, repositoryUri, project, repoName, remoteInfo);
        }

        private RepositorySettings CreateRepositorySettings(string org, Uri repositoryUri, string project, string repoName, RemoteInfo remoteInfo = null) => new RepositorySettings
        {
            ApiUri = new Uri($"https://dev.azure.com/{org}/"),
            RepositoryUri = repositoryUri,
            RepositoryName = repoName,
            RepositoryOwner = project,
            RemoteInfo = remoteInfo
        };
    }
}
