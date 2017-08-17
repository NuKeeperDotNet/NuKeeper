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
        private readonly Credentials _gitCredentials;

        public IFolder WorkingFolder { get; }

        public LibGit2SharpDriver(INuKeeperLogger logger,  
            IFolder workingFolder, Credentials gitCredentials)
        {
            if (gitCredentials == null)
            {
                throw new ArgumentNullException(nameof(gitCredentials));
            }


            _logger = logger;
            WorkingFolder = workingFolder;
            _gitCredentials = gitCredentials;
        }
        public void Clone(Uri pullEndpoint)
        {
            _logger.Verbose($"Git clone {pullEndpoint} to {WorkingFolder.FullPath}");

            Repository.Clone(pullEndpoint.ToString(), WorkingFolder.FullPath,
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
            return new Repository(WorkingFolder.FullPath);
        }

        private Credentials UsernamePasswordCredentials(
            string url, string usernameFromUrl, SupportedCredentialTypes types)
        {
            return _gitCredentials;
        }
    }
}