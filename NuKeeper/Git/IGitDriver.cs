using System;
using System.Threading.Tasks;

namespace NuKeeper.Git
{
    interface IGitDriver
    {
        Task Clone(Uri pullEndpoint);

        Task Checkout(string branchName);

        Task CheckoutNewBranch(string branchName);

        Task Commit(string message);

        Task Push(string remoteName, string branchName);
    }
}
