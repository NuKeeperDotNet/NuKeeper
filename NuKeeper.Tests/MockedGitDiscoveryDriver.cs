using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.Tests
{
    public class MockedGitDiscoveryDriver : IGitDiscoveryDriver
    {
        public Task<bool> IsGitRepo(Uri repositoryUri)
        {
            return Task.FromResult(true);
        }

        public Task<IEnumerable<GitRemote>> GetRemotes(Uri repositoryUri)
        {
            return Task.FromResult<IEnumerable<GitRemote>>(new List<GitRemote>(){ new GitRemote
            {
                Name="origin",
                Url = repositoryUri
            }});
        }

        public Task<Uri> DiscoverRepo(Uri repositoryUri)
        {
            return Task.FromResult(repositoryUri);
        }

        public Task<string> GetCurrentHead(Uri repositoryUri)
        {
            return Task.FromResult("master");
        }

        public async Task<GitRemote> GetRemoteForPlatform(Uri repositoryUri, string platformHost)
        {
            return (await GetRemotes(repositoryUri)).First();
        }
    }
}
