using System;
using System.Linq;
using System.Threading.Tasks;
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

        public AzureDevOpsSettingsReader(IGitDiscoveryDriver gitDriver, IEnvironmentVariablesProvider environmentVariablesProvider)
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

            // Did we specify a Azure DevOps url?
            return repositoryUri?.Host.Contains(PlatformHost, StringComparison.OrdinalIgnoreCase) == true;
        }

        public override async Task<RepositorySettings> RepositorySettings(Uri repositoryUri, string targetBranch)
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
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri} and format should be {UrlPattern}");
            }

            return settings;
        }

        private RepositorySettings CreateSettingsFromRemote(Uri repositoryUri)
        {
            // URL pattern is 
            // https://dev.azure.com/{org}/{project}/_git/{repo}/
            // for a organisation or
            // https://dev.azure.com/{owner}/_git/{repo}
            // for a private repository
            var path = repositoryUri.AbsolutePath;

            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            var indexOfGit = Array.FindIndex(pathParts, t => t.Equals("_git", StringComparison.InvariantCultureIgnoreCase));

            if (indexOfGit == 2 && pathParts.Length == 4)
            {
                return CreateRepositorySettings(
                    pathParts[0],  //org
                    repositoryUri, //uri
                    Uri.UnescapeDataString(pathParts[1]),  // project
                    Uri.UnescapeDataString(pathParts[3])   // reponame
                    );
            }
            else if (indexOfGit == 1 && pathParts.Length == 3)
            {
                return CreateRepositorySettings(
                    null,          //org
                    repositoryUri, //uri
                    Uri.UnescapeDataString(pathParts[0]),  // project
                    Uri.UnescapeDataString(pathParts[2])   // reponame
                    );
            }
            return null;
        }

        private async Task<RepositorySettings> CreateSettingsFromLocal(Uri repositoryUri, string targetBranch)
        {
            var remoteInfo = new RemoteInfo();

            var localCopy = repositoryUri;
            if (await _gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var origin = await _gitDriver.GetRemoteForPlatform(repositoryUri, PlatformHost);

                if (origin != null)
                {
                    remoteInfo.LocalRepositoryUri = await _gitDriver.DiscoverRepo(localCopy); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    remoteInfo.WorkingFolder = localCopy;
                    remoteInfo.BranchName = targetBranch ?? await _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
                    remoteInfo.RemoteName = origin.Name;
                }
            }
            else
            {
                throw new NuKeeperException("No git repository found");
            }

            // URL pattern is 
            // https://dev.azure.com/{org}/{project}/_git/{repo}/
            // for a organisation or
            // https://dev.azure.com/{owner}/_git/{repo}
            // for a private repository
            var path = repositoryUri.AbsolutePath;

            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            var indexOfGit = Array.FindIndex(pathParts, t => t.Equals("_git", StringComparison.InvariantCultureIgnoreCase));

            if (indexOfGit == 2 && pathParts.Length == 4)
            {
                return CreateRepositorySettings(
                    pathParts[0],  //org
                    repositoryUri, //uri
                    Uri.UnescapeDataString(pathParts[1]),  // project
                    Uri.UnescapeDataString(pathParts[3])   // reponame
                    );
            }
            else if (indexOfGit == 1 && pathParts.Length == 3)
            {
                return CreateRepositorySettings(
                    null,          //org
                    repositoryUri, //uri
                    Uri.UnescapeDataString(pathParts[0]),  // project
                    Uri.UnescapeDataString(pathParts[2])   // reponame
                    );
            }
            return null;
        }

        private static RepositorySettings CreateRepositorySettings(string org, Uri repositoryUri, string project, string repoName, RemoteInfo remoteInfo = null) => new RepositorySettings
        {
            ApiUri = string.IsNullOrWhiteSpace(org) ? new Uri($"https://dev.azure.com/") : new Uri($"https://dev.azure.com/{org}/"),
            RepositoryUri = repositoryUri,
            RepositoryName = repoName,
            RepositoryOwner = project,
            RemoteInfo = remoteInfo
        };
    }
}
