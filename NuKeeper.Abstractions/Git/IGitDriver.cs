using System;
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

        Task Checkout(string branchName);

        Task CheckoutNewBranch(string branchName);

        Task Commit(string message);

        Task Push(string remoteName, string branchName);

        Task<string> GetCurrentHead();

    }
}
