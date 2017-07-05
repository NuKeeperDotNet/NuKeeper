using System;
using System.Threading.Tasks;

namespace NuKeeper.Git
{
    class Git : IGit
    {
        public Task Pull(Uri pullEndpoint)
        {
            throw new NotImplementedException();
        }

        public void Checkout(string branchName)
        {
            throw new NotImplementedException();
        }

        public void Commit()
        {
            throw new NotImplementedException();
        }

        public void Push(string remoteName, string branchName)
        {
            throw new NotImplementedException();
        }
    }
}