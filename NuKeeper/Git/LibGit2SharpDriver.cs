using System;
using System.Linq;
using LibGit2Sharp;
using NuKeeper.Types.Files;
using NuKeeper.Types.Logging;

namespace NuKeeper.Git
{
    public class LibGit2SharpDriver : IGitDriver
    {
        private readonly INuKeeperLogger _logger;
        private readonly Credentials _gitCredentials;
        private readonly Identity _identity;
        private bool _fetchFinished;

        public IFolder WorkingFolder { get; }

        public LibGit2SharpDriver(INuKeeperLogger logger,
            IFolder workingFolder, Credentials gitCredentials, Identity userIdentity)
        {
            if (workingFolder == null)
            {
                throw new ArgumentNullException(nameof(workingFolder));
            }

            if (gitCredentials == null)
            {
                throw new ArgumentNullException(nameof(gitCredentials));
            }

            _logger = logger;
            WorkingFolder = workingFolder;
            _gitCredentials = gitCredentials;
            _identity = userIdentity;
        }
        public void Clone(Uri pullEndpoint)
        {
            _logger.Info($"Git clone {pullEndpoint} to {WorkingFolder.FullPath}");

            Repository.Clone(pullEndpoint.ToString(), WorkingFolder.FullPath,
                new CloneOptions
                {
                    CredentialsProvider = UsernamePasswordCredentials,
                    OnTransferProgress = OnTransferProgress
                });

            _logger.Verbose("Git clone complete");
        }

        private bool OnTransferProgress(TransferProgress progress)
        {
            if (progress.ReceivedObjects % (progress.TotalObjects / 10) == 0 && !_fetchFinished)
            {
                _logger.Verbose($"{progress.ReceivedObjects} / {progress.TotalObjects}");
                _fetchFinished = progress.ReceivedObjects == progress.TotalObjects;
            }

            return true;
        }

        public void AddRemote(string name, Uri endpoint)
        {
            using (var repo = MakeRepo())
            {
                repo.Network.Remotes.Add(name, endpoint.ToString());
            }
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
            var qualifiedBranchName = "origin/" + branchName;
            if (BranchExists(qualifiedBranchName))
            {
                throw new Exception($"Git Cannot checkout new branch: a branch named '{qualifiedBranchName}' already exists");
            }

            _logger.Verbose($"Git checkout new branch '{branchName}'");
            using (var repo = MakeRepo())
            {
                var branch = repo.CreateBranch(branchName);
                Commands.Checkout(repo, branch);
            }
        }

        public bool BranchExists(string branchName)
        {
            using (var repo = MakeRepo())
            {
                var branchFound = repo.Branches.Any(
                    br => string.Equals(br.FriendlyName, branchName, StringComparison.Ordinal));
                return branchFound;
            }
        }

        public void Commit(string message)
        {
            _logger.Verbose($"Git commit with message '{message}'");
            using (var repo = MakeRepo())
            {
                var signature = GetSignature(repo);
                Commands.Stage(repo, "*");
                repo.Commit(message, signature, signature);
            }
        }

        private Signature GetSignature(Repository repo)
        {
            if (_identity != null)
            {
                return new Signature(_identity, DateTimeOffset.Now);
            }

            var repoSignature =  repo.Config.BuildSignature(DateTimeOffset.Now);

            if (repoSignature == null)
            {
                throw new InvalidOperationException(
                    "Failed to build signature, did not get valid git user identity from github token or from repo config");
            }

            return repoSignature;
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
