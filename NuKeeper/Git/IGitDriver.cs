using System;
using NuKeeper.Files;

namespace NuKeeper.Git
{
    public interface IGitDriver
    {
        IFolder WorkingFolder { get; }

        void Clone(Uri pullEndpoint);

        void Checkout(string branchName);

        void CheckoutNewBranch(string branchName);

        bool BranchExists(string branchName);

        void Commit(string message);

        void Push(string remoteName, string branchName);

        string GetCurrentHead();
    }
}
