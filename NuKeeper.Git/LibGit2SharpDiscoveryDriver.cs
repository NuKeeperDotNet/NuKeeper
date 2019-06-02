using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Git
{
    public class LibGit2SharpDiscoveryDriver : IGitDiscoveryDriver
    {
        private readonly INuKeeperLogger _logger;

        public LibGit2SharpDiscoveryDriver(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public async Task<bool> IsGitRepo(Uri repositoryUri)
        {
            var discovered = await DiscoverRepo(repositoryUri);
            if (discovered == null)
            {
                return false;
            }

            return Repository.IsValid(discovered.AbsolutePath);
        }

        public async Task<IEnumerable<GitRemote>> GetRemotes(Uri repositoryUri)
        {
            if (!await IsGitRepo(repositoryUri))
            {
                return Enumerable.Empty<GitRemote>();
            }

            var discover = Repository.Discover(repositoryUri.AbsolutePath);

            var gitRemotes = new List<GitRemote>();
            using (var repo = new Repository(discover))
            {
                foreach (var remote in repo.Network.Remotes)
                {
                    Uri.TryCreate(remote.Url, UriKind.Absolute, out repositoryUri);

                    if (repositoryUri != null)
                    {
                        var gitRemote = new GitRemote
                        {
                            Name = remote.Name,
                            Url = repositoryUri
                        };
                        gitRemotes.Add(gitRemote);
                    }
                    else
                    {
                        _logger.Normal($"Cannot parse {remote.Url} to URI. SSH remote is currently not supported");
                    }
                }

                return gitRemotes;
            }
        }

        public Task<Uri> DiscoverRepo(Uri repositoryUri)
        {
            return Task.Run(() =>
            {
                var discovery = Repository.Discover(repositoryUri.AbsolutePath);

                if (string.IsNullOrEmpty(discovery))
                {
                    return null;
                }

                return new Uri(discovery);
            });
        }

        public async Task<string> GetCurrentHead(Uri repositoryUri)
        {
            var repoRoot = (await DiscoverRepo(repositoryUri)).AbsolutePath;
            using (var repo = new Repository(repoRoot))
            {
                var repoHeadBranch = repo.Branches.
                    SingleOrDefault(b => b.IsCurrentRepositoryHead);

                if (repoHeadBranch == null)
                {
                    throw new NuKeeperException($"Cannot find current head branch for repo at '{repoRoot}', with {repo.Branches.Count()} branches");
                }

                return repoHeadBranch.FriendlyName;
            }
        }

        public async Task<GitRemote> GetRemoteForPlatform(Uri repositoryUri, string platformHost)
        {
            var remotes = await GetRemotes(repositoryUri);
            return remotes
                .FirstOrDefault(rm => rm.Url.Host.Contains(platformHost, StringComparison.OrdinalIgnoreCase));
        }
    }
}
