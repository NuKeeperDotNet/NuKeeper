using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using System;
using System.Linq;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.GitHub
{
    public class GitHubSettingsReader : ISettingsReader
    {
        private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
        private const string PlatformHost = "github.com";
        private const string UrlPattern = "https://github.com/{owner}/{reponame}.git";
        private readonly IGitDiscoveryDriver _gitDriver;
        private bool _isLocalGitRepo;


        public GitHubSettingsReader(IGitDiscoveryDriver gitDriver, IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider;
            _gitDriver = gitDriver;
        }

        public Platform Platform => Platform.GitHub;

        public bool CanRead(Uri repositoryUri)
        {
            // Is the specified folder already a git repository?
            if (repositoryUri.IsFile)
            {
                repositoryUri = repositoryUri.GetRemoteUriFromLocalRepo(_gitDriver, PlatformHost);
                _isLocalGitRepo = true;
            }

            return repositoryUri?.Host.Contains(PlatformHost, StringComparison.OrdinalIgnoreCase) == true;
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            var envToken = _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_github_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
            settings.ForkMode = settings.ForkMode ?? ForkMode.PreferFork;
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch = null)
        {
            var settings = _isLocalGitRepo ? CreateSettingsFromLocal(repositoryUri, targetBranch) : CreateSettingsFromRemote(repositoryUri, targetBranch);
            if (settings == null)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri.ToString()} and format should be {UrlPattern}");
            }

            return settings;
        }

        private RepositorySettings CreateSettingsFromLocal(Uri repositoryUri, string targetBranch)
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
                    remoteInfo.BranchName = targetBranch ?? _gitDriver.GetCurrentHead(remoteInfo.LocalRepositoryUri);
                    remoteInfo.RemoteName = origin.Name;
                    remoteInfo.WorkingFolder = localFolder;
                }
            }
            else
            {
                throw new NuKeeperException("No git repository found");
            }

            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
            {
                ApiUri = new Uri("https://api.github.com/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                RemoteInfo = targetBranch != null ? new RemoteInfo { BranchName = targetBranch } : null
            };
        }

        private RepositorySettings CreateSettingsFromRemote(Uri repositoryUri, string targetBranch)
        {
            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri.ToString()} and format should be {UrlPattern}");
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
            {
                ApiUri = new Uri("https://api.github.com/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                RemoteInfo = targetBranch != null ? new RemoteInfo { BranchName = targetBranch } : null
            };
        }
    }
}
