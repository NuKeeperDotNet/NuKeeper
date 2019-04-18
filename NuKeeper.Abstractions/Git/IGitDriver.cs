using System;
using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Abstractions.Git
{
    public interface IGitDriver
    {
        IFolder WorkingFolder { get; }

        void Clone(Uri pullEndpoint);

        void Clone(Uri pullEndpoint, string branchName);

        void AddRemote(string name, Uri endpoint);

        void Checkout(string branchName);

        void CheckoutNewBranch(string branchName);

        void Commit(string message);

        void Push(string remoteName, string branchName);

        string GetCurrentHead();

    }
}
