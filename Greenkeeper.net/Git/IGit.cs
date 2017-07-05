using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Greenkeeper.Git
{
    interface IGit
    {
        Task Pull(Uri pullEndpoint);

        void Checkout(string branchName);

        void Commit();

        void Push(string remoteName, string branchName);
    }
}
