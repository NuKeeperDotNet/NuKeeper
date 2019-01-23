using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Abstractions.Git
{
    public interface IGitDriver
    {
        IFolder WorkingFolder { get; }

        Task Clone(Uri pullEndpoint);

        void AddRemote(string name, Uri endpoint);

        void Checkout(string branchName);

        void CheckoutNewBranch(string branchName);

        void Commit(string message);

        Task Push(string remoteName, string branchName);

        string GetCurrentHead();

    }
}
