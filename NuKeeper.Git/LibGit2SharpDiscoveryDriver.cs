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

        public IEnumerable<GitRemote> GetRemotes(Uri repositoryUri)
        {
            if (! IsGitRepo(repositoryUri))
            {
                return Enumerable.Empty<GitRemote>();
            }
            
            var discover = Repository.Discover(repositoryUri.AbsolutePath);

            var repo = new Repository(discover);
            
            var gitRemotes = new List<GitRemote>();

            foreach (var remote in repo.Network.Remotes)
            {
                var gitRemote = new GitRemote
                {
                    Name = remote.Name,
                    Url = new Uri(remote.Url)
                };
                gitRemotes.Add(gitRemote);
            }
            
            return gitRemotes;
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
            var origin = remotes.FirstOrDefault(rm => rm.Url.Host.ContainsOrdinal(platformHost) == true);
            return origin;
        }
    }


}
