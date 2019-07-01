using NuKeeper.Abstractions.Inspections.Files;
using System;

namespace NuKeeper.Abstractions.Git
{
    public interface IGitDriver
    {
        IFolder WorkingFolder { get; }

        void Clone(Uri pullEndpoint);

        void Clone(Uri pullEndpoint, string branchName);

        void AddRemote(string name, Uri endpoint);

        /// <summary>
        /// Checks out a local branch
        /// </summary>
        /// <param name="branchName"></param>
        void Checkout(string branchName);

        /// <summary>
        /// does a checkout of a remote branch
        /// </summary>
        /// <param name="branchName"></param>
        void CheckoutRemoteToLocal(string branchName);

        /// <summary>
        /// Creates a new branch
        /// </summary>
        /// <param name="branchName"></param>
        /// <returns></returns>
        bool CheckoutNewBranch(string branchName);

        void Commit(string message);

        void Push(string remoteName, string branchName);

        string GetCurrentHead();

        /// <summary>
        /// Gets the commit messages that are in branch <paramref name="headBranchName"/> but not in branch <paramref name="baseBranchName"/>
        /// </summary>
        /// <param name="baseBranchName"></param>
        /// <param name="headBranchName"></param>
        /// <returns></returns>
        string[] GetNewCommitMessages(string baseBranchName, string headBranchName);
    }
}
