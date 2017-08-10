using System;
using System.Linq;
using LibGit2Sharp;
using NuKeeper.Files;
using NuKeeper.Logging;

namespace NuKeeper.Git
{
    public class LibGit2SharpDriver : IGitDriver
    {
        private readonly INuKeeperLogger _logger;
        private readonly IFolder _tempFolder;
        private readonly string _githubUser;
        private readonly string _githubToken;

        public LibGit2SharpDriver(INuKeeperLogger logger,  
            IFolder tempFolder, string githubUser, string githubToken)
        {
            if (string.IsNullOrWhiteSpace(githubUser))
            {
                throw new ArgumentNullException(nameof(githubUser));
            }

            if (string.IsNullOrWhiteSpace(githubToken))
            {
                throw new ArgumentNullException(nameof(githubToken));
            }

            _logger = logger;
            _tempFolder = tempFolder;
            _githubUser = githubUser;
            _githubToken = githubToken;
        }
        public void Clone(Uri pullEndpoint)
        {
            _logger.Verbose($"Git clone {pullEndpoint} to {_tempFolder.FullPath}");

            Repository.Clone(pullEndpoint.ToString(), _tempFolder.FullPath,
                new CloneOptions
                {
                    CredentialsProvider = UsernamePasswordCredentials
                });

            _logger.Verbose("Git clone complete");
        }

        public void Checkout(string branchName)
        {
            _logger.Verbose($"Git checkout '{branchName}'");
            using (var repo = MakeRepo())
            {
                Commands.Checkout(repo, repo.Branches[branchName]);
            }
        }

        public void CheckoutNewBranch(string branchName)
        {
            _logger.Verbose($"Git checkout new branch '{branchName}'");
            using (var repo = MakeRepo())
            {
                var branch = repo.CreateBranch(branchName);
                Commands.Checkout(repo, branch);
            }
        }

        public void Commit(string message)
        {
            _logger.Verbose($"Git commit with message '{message}'");
            using (var repo = MakeRepo())
            {
                var sig = repo.Config.BuildSignature(DateTimeOffset.Now);
                Commands.Stage(repo, "*");
                repo.Commit(message, sig, sig);
            }
        }

        public void Push(string remoteName, string branchName)
        {
            _logger.Verbose($"Git push to {remoteName}/{branchName}");

            using (var repo = MakeRepo())
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

        public string GetCurrentHead()
        {
            using (var repo = MakeRepo())
            {
                return repo.Branches.Single(b => b.IsCurrentRepositoryHead).FriendlyName;
            }
        }

        private Repository MakeRepo()
        {
            return new Repository(_tempFolder.FullPath);
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