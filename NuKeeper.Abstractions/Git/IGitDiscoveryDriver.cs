using System;
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
        IReadOnlyCollection<GitRemote> GetRemotes(Uri repositoryUri);

        Uri DiscoverRepo(Uri repositoryUri);

        string GetCurrentHead(Uri repositoryUri);

        GitRemote GetRemoteForPlatform(Uri repositoryUri, string platformHost);
    }
}
