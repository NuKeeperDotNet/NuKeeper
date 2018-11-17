using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Abstractions.Git
{
    public interface IGitDiscoveryDriver
    {
        /// <summary>
        /// Checks if the specified folder is a git folder.
        /// </summary>
        /// <param name="repositoryUri"></param>
        /// <returns></returns>
        bool IsGitRepo(Uri repositoryUri);

        /// <summary>
        /// Get all the configured remotes
        /// </summary>
        /// <param name="repositoryUri"></param>
        IEnumerable<GitRemote> GetRemotes(Uri repositoryUri);

        Uri DiscoverRepo(Uri repositoryUri);

        string GetCurrentHead(Uri repositoryUri);

        GitRemote GetRemoteForPlatform(Uri repositoryUri, string platformHost);
    }

    public class GitRemote
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
    }
}
