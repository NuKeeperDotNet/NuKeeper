using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.Tests
{
    public class MockedGitDiscoveryDriver : IGitDiscoveryDriver
    {
        public bool IsGitRepo(Uri repositoryUri)
        {
            return true;
        }

        public IEnumerable<GitRemote> GetRemotes(Uri repositoryUri)
        {
            return new List<GitRemote>(){ new GitRemote
            {
                Name="origin",
                Url = repositoryUri
            }};
        }

        public Uri DiscoverRepo(Uri repositoryUri)
        {
            return repositoryUri;
        }

        public string GetCurrentHead(Uri repositoryUri)
        {
            return "master";
        }

        public GitRemote GetRemoteForPlatform(Uri repositoryUri, string platformHost)
        {
            return GetRemotes(repositoryUri).First();
        }
    }
}
