using System;
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
        Task<bool> IsGitRepo(Uri repositoryUri);

        /// <summary>
        /// Get all the configured remotes
        /// </summary>
        /// <param name="repositoryUri"></param>
        Task<IEnumerable<GitRemote>> GetRemotes(Uri repositoryUri);

        Task<Uri> DiscoverRepo(Uri repositoryUri);

        Task<string> GetCurrentHead(Uri repositoryUri);

        Task<GitRemote> GetRemoteForPlatform(Uri repositoryUri, string platformHost);
    }
}
