using System;
using System.Collections;
using System.Collections.Generic;

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
    }

    public class GitRemote
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
    }
}
