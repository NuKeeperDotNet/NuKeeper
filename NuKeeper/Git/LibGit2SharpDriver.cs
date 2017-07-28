using System;
using LibGit2Sharp;

namespace NuKeeper.Git
{
    public class LibGit2SharpDriver : IGitDriver
    {
        private readonly string _repoStoragePath;
        private readonly string _githubUser;
        private readonly string _githubToken;

        public LibGit2SharpDriver(string repoStoragePath, string githubUser, string githubToken)
        {
            _repoStoragePath = repoStoragePath;
            _githubUser = githubUser;
            _githubToken = githubToken;
        }
        public void Clone(Uri pullEndpoint)
        {
            Repository.Clone(pullEndpoint.ToString(), _repoStoragePath,
                new CloneOptions
                {
                    CredentialsProvider = UsernamePasswordCredentials
                });
        }

        public void Checkout(string branchName)
        {
            using (var repo = new Repository(_repoStoragePath))
            {
                Commands.Checkout(repo, repo.Branches[branchName]);
            }
        }

        public void CheckoutNewBranch(string branchName)
        {
            using (var repo = new Repository(_repoStoragePath))
            {
                var branch = repo.CreateBranch(branchName);
                Commands.Checkout(repo, branch);
            }
        }

        public void Commit(string message)
        {
            using (var repo = new Repository(_repoStoragePath))
            {
                var sig = repo.Config.BuildSignature(DateTimeOffset.Now);
                Commands.Stage(repo, "*");
                repo.Commit(message, sig, sig);
            }
        }

        public void Push(string remoteName, string branchName)
        {
            using (var repo = new Repository(_repoStoragePath))
            {
                var localBranch = repo.Branches[branchName];
                var remote = repo.Network.Remotes[remoteName];

                repo.Branches.Update(localBranch,
                    b => b.Remote = remote.Name,
                    b => b.UpstreamBranch = localBranch.CanonicalName);

                repo.Network.Push(localBranch, new PushOptions
                {
                    CredentialsProvider = UsernamePasswordCredentials
                });
            }
        }

        private UsernamePasswordCredentials UsernamePasswordCredentials(
            string url, string usernameFromUrl, 
            SupportedCredentialTypes types)
        {
            return new UsernamePasswordCredentials
            {

                Username = _githubUser,
                Password = _githubToken
            };
        }
    }
}