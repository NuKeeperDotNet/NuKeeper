using System;
using System.Threading.Tasks;

namespace NuKeeper.Git
{
    interface IGit
    {
        Task Clone(Uri pullEndpoint);

        Task Checkout(string branchName);

        Task Commit(string message);

        Task Push(string remoteName, string branchName);
    }
}
