using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.Git
{
    public class LibGit2SharpDiscoveryDriver : IGitDiscoveryDriver
    {
        public bool IsGitRepo(Uri repositoryUri)
        {
            var discovered = DiscoverRepo(repositoryUri);
            if (discovered == null)
            {
                return false;
            }

            return Repository.IsValid(discovered.AbsolutePath);
        }

        public IReadOnlyCollection<GitRemote> GetRemotes(Uri repositoryUri)
        {
            if (! IsGitRepo(repositoryUri))
            {
                return Array.Empty<GitRemote>();
            }
            
            var discover = Repository.Discover(repositoryUri.AbsolutePath);

            var repo = new Repository(discover);

            return repo.Network.Remotes
                .Select(ReadRemote)
                .ToList();
        }

        private GitRemote ReadRemote(Remote remote)
        {
            var url = remote.Url;
            if (url.StartsWith("git@", StringComparison.OrdinalIgnoreCase))
            {
                url = "ssh:" + url;
            }

            return new GitRemote
            {
                Name = remote.Name,
                Url = new Uri(url)
            };
        }

        public Uri DiscoverRepo(Uri repositoryUri)
        {
            var discovery = Repository.Discover(repositoryUri.AbsolutePath);

            if (string.IsNullOrEmpty(discovery))
            {
                return null;
            }

            return new Uri(discovery);
        }

        public string GetCurrentHead(Uri repositoryUri)
        {
            using (var repo = new Repository(DiscoverRepo(repositoryUri).AbsolutePath))
            {
                var repoHead = repo.Branches.Single(b => b.IsCurrentRepositoryHead);
                return repoHead.FriendlyName;
            }
        }

        public GitRemote GetRemoteForPlatform(Uri repositoryUri, string platformHost)
        {
            var remotes = GetRemotes(repositoryUri);
            return remotes
                .FirstOrDefault(rm => rm.Url.Host.Contains(platformHost, StringComparison.OrdinalIgnoreCase));
        }
    }


}
