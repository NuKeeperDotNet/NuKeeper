using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Abstractions.Git
{
    public interface IGitDriver
    {
        IFolder WorkingFolder { get; }

        Task Clone(Uri pullEndpoint);

        Task Clone(Uri pullEndpoint, string branchName);

        Task AddRemote(string name, Uri endpoint);

        /// <summary>
        /// Checks out a local branch
        /// </summary>
        /// <param name="branchName"></param>
        Task Checkout(string branchName);

        /// <summary>
        /// does a checkout of a remote branch
        /// </summary>
        /// <param name="branchName"></param>
        Task CheckoutRemoteToLocal(string branchName);

        /// <summary>
        /// Creates a new branch
        /// </summary>
        /// <param name="branchName"></param>
        /// <returns></returns>
        Task<bool> CheckoutNewBranch(string branchName);

        Task Commit(string message);

        Task Push(string remoteName, string branchName);

        Task<string> GetCurrentHead();

        /// <summary>
        /// Gets the commit messages that are in branch <paramref name="headBranchName"/> but not in branch <paramref name="baseBranchName"/>
        /// </summary>
        /// <param name="baseBranchName"></param>
        /// <param name="headBranchName"></param>
        /// <returns></returns>
        Task<IEnumerable<string>> GetNewCommitMessages(string baseBranchName, string headBranchName);
    }
}
