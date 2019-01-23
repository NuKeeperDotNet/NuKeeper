using System;

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

        Uri DiscoverRepo(Uri repositoryUri);

        string GetCurrentHead(Uri repositoryUri);

        GitRemote GetRemoteForPlatform(Uri repositoryUri, string platformHost);
    }
}
