using System;
using System.Threading.Tasks;

namespace NuKeeper.Git
{
    interface IGit
    {
        Task Pull(Uri pullEndpoint);

        void Checkout(string branchName);

        void Commit();

        void Push(string remoteName, string branchName);
    }
}
