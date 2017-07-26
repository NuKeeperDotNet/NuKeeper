using System;
using System.Threading.Tasks;

namespace NuKeeper.Git
{
    public interface IGitDriver
    {
        void Clone(Uri pullEndpoint);

        void Checkout(string branchName);

        void CheckoutNewBranch(string branchName);

        void Commit(string message);

        void Push(string remoteName, string branchName);
    }
}
