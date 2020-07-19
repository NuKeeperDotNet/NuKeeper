using System;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.AzureDevOps
{
    public class TfsSettingsReader : BaseSettingsReader
    {
        private const string PlatformHost = "tfs";
        private const string UrlPattern = "https://tfs.company.local:{port}/<nothingOrVirtualSite>/{project}/_git/{repo}";

        private readonly IGitDiscoveryDriver _gitDriver;

        public TfsSettingsReader(IGitDiscoveryDriver gitDriver, IEnvironmentVariablesProvider environmentVariablesProvider)
        : base(environmentVariablesProvider)
        {
            _gitDriver = gitDriver;
        }

        public override async Task<bool> CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return false;
            }

            // Is the specified folder already a git repository?
            if (repositoryUri.IsFile)
            {
                repositoryUri = await repositoryUri.GetRemoteUriFromLocalRepo(_gitDriver, PlatformHost);
            }

            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var tfsInPath = pathParts.Count > 0 && pathParts[0].Contains(PlatformHost, StringComparison.OrdinalIgnoreCase);
            var tfsInHost = repositoryUri.Host.Contains(PlatformHost, StringComparison.OrdinalIgnoreCase);
            return tfsInPath || tfsInHost;
        }

        public override async Task<RepositorySettings> RepositorySettings(Uri repositoryUri, bool setAutoComplete, string targetBranch = null)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            var settings = repositoryUri.IsFile
                ? await CreateSettingsFromLocal(repositoryUri, targetBranch)
                : CreateSettingsFromRemote(repositoryUri);
            if (settings == null)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri.ToString()} and format should be {UrlPattern}");
            }

            settings.SetAutoComplete = setAutoComplete;

            return settings;
        }

        private static RepositorySettings CreateSettingsFromRemote(Uri repositoryUri)
        {
            return RepositorySettings(repositoryUri);
        }

        private async Task<RepositorySettings> CreateSettingsFromLocal(Uri repositoryUri, string targetBranch)
        {
            var remoteInfo = new RemoteInfo();

            var localFolder = repositoryUri;
            if (await _gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var origin = await _gitDriver.GetRemoteForPlatform(repositoryUri, PlatformHost);

                if (origin != null)
                {
                    remoteInfo.LocalRepositoryUri = await _gitDriver.DiscoverRepo(repositoryUri); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    remoteInfo.BranchName = targetBranch ?? await _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
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

            var project = Uri.UnescapeDataString(pathParts[gitLocation - 1]);
            var repoName = Uri.UnescapeDataString(pathParts[gitLocation + 1]);
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
