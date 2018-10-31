using System;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Git
{
    public interface IGitDriver
    {
        IFolder WorkingFolder { get; }

        void Clone(Uri pullEndpoint);

        void AddRemote(string name, Uri endpoint);

        void Checkout(string branchName);

        void CheckoutNewBranch(string branchName);

        void Commit(string message);

        void Push(string remoteName, string branchName);

        string GetCurrentHead();
    }
}
