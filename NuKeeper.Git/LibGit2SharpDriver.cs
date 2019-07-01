using LibGit2Sharp;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using System;
using System.Linq;
using GitCommands = LibGit2Sharp.Commands;
using Repository = LibGit2Sharp.Repository;

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
            IFolder workingFolder, GitUsernamePasswordCredentials credentials, User user)
        {
            if (workingFolder == null)
            {
                throw new ArgumentNullException(nameof(workingFolder));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            _logger = logger;
            WorkingFolder = workingFolder;
            _gitCredentials = new UsernamePasswordCredentials
            { Password = credentials.Password, Username = credentials.Username };
            _identity = GetUserIdentity(user);
        }

        public void Clone(Uri pullEndpoint)
        {
            Clone(pullEndpoint, null);
        }

        public void Clone(Uri pullEndpoint, string branchName)
        {
            _logger.Normal($"Git clone {pullEndpoint}, branch {branchName ?? "default"}, to {WorkingFolder.FullPath}");

            Repository.Clone(pullEndpoint.AbsoluteUri, WorkingFolder.FullPath,
                new CloneOptions
                {
                    CredentialsProvider = UsernamePasswordCredentials,
                    OnTransferProgress = OnTransferProgress,
                    BranchName = branchName
                });

            _logger.Detailed("Git clone complete");
        }

        private bool OnTransferProgress(TransferProgress progress)
        {
            if (progress.ReceivedObjects % (Math.Max(progress.TotalObjects / 10, 1)) == 0 && !_fetchFinished)
            {
                _logger.Detailed($"{progress.ReceivedObjects} / {progress.TotalObjects}");
                _fetchFinished = progress.ReceivedObjects == progress.TotalObjects;
            }

            return true;
        }

        public void AddRemote(string name, Uri endpoint)
        {
            using (var repo = MakeRepo())
            {
                repo.Network.Remotes.Add(name, endpoint.AbsoluteUri);
            }
        }

        public void Checkout(string branchName)
        {
            _logger.Detailed($"Git checkout '{branchName}'");
            using (var repo = MakeRepo())
            {
                if (BranchExists(branchName))
                {
                    _logger.Normal($"Git checkout local branch '{branchName}'");
                    // Some files are automatically generated in /obj folder.
                    // If there is no .gitignore there will be conflicts because of this and you cannot change branches
                    // CheckoutModifiers.Force makes that all changes are ignored.
                    GitCommands.Checkout(repo, repo.Branches[branchName], new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
                }
                else
                {
                    throw new NuKeeperException(
                        $"Git Cannot checkout branch: the branch named '{branchName}' doesn't exist");
                }
            }
        }

        public void CheckoutRemoteToLocal(string branchName)
        {
            var qualifiedBranchName = "origin/" + branchName;

            _logger.Detailed($"Git checkout '{qualifiedBranchName}'");
            using (var repo = MakeRepo())
            {
                if (!BranchExists(qualifiedBranchName))
                {
                    throw new NuKeeperException(
                        $"Git Cannot checkout branch: the branch named '{qualifiedBranchName}' doesn't exist");
                }

                if (BranchExists(branchName))
                {
                    throw new NuKeeperException(
                        $"Git Cannot checkout branch '{qualifiedBranchName}' to '{branchName}': the branch named '{branchName}' does already exist");
                }

                _logger.Normal($"Git checkout existing branch '{qualifiedBranchName}' to '{branchName}'");

                // Get a reference on the remote tracking branch
                var trackedBranch = repo.Branches[qualifiedBranchName];
                // Create a local branch pointing at the same Commit
                var branch = repo.CreateBranch(branchName, trackedBranch.Tip);
                // Configure the local branch to track the remote one.
                repo.Branches.Update(branch, b => b.TrackedBranch = trackedBranch.CanonicalName);

                // go to the just created branch
                Checkout(branchName);
            }
        }

        public bool CheckoutNewBranch(string branchName)
        {
            var qualifiedBranchName = "origin/" + branchName;
            if (BranchExists(qualifiedBranchName))
            {
                _logger.Normal($"Git Cannot checkout new branch: a branch named '{qualifiedBranchName}' already exists");
                return false;
            }

            _logger.Detailed($"Git checkout new branch '{branchName}'");
            using (var repo = MakeRepo())
            {
                var branch = repo.CreateBranch(branchName);
                GitCommands.Checkout(repo, branch);
            }
            return true;
        }

        private bool BranchExists(string branchName)
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
            _logger.Detailed($"Git commit with message '{message}'");
            using (var repo = MakeRepo())
            {
                var signature = GetSignature(repo);
                GitCommands.Stage(repo, "*");
                repo.Commit(message, signature, signature);
            }
        }

        private Signature GetSignature(Repository repo)
        {
            if (_identity != null)
            {
                return new Signature(_identity, DateTimeOffset.Now);
            }

            var repoSignature = repo.Config.BuildSignature(DateTimeOffset.Now);

            if (repoSignature == null)
            {
                throw new NuKeeperException(
                    "Failed to build signature, did not get valid git user identity from token or from repo config");
            }

            return repoSignature;
        }

        public void Push(string remoteName, string branchName)
        {
            _logger.Detailed($"Git push to {remoteName}/{branchName}");

            using (var repo = MakeRepo())
            {

                var localBranch = repo.Branches
                    .Single(b => b.CanonicalName.EndsWith(branchName, StringComparison.OrdinalIgnoreCase) && !b.IsRemote);
                var remote = repo.Network.Remotes
                    .Single(r => r.Name.EndsWith(remoteName, StringComparison.OrdinalIgnoreCase));

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


        private Identity GetUserIdentity(User user)
        {
            if (string.IsNullOrWhiteSpace(user?.Name))
            {
                _logger.Minimal("User name missing from profile, falling back to .gitconfig");
                return null;
            }
            if (string.IsNullOrWhiteSpace(user?.Email))
            {
                _logger.Minimal("Email missing from profile, falling back to .gitconfig");
                return null;
            }

            return new Identity(user.Name, user.Email);
        }

        public string[] GetNewCommitMessages(string baseBranchName, string headBranchName)
        {
            if (!BranchExists(baseBranchName))
            {
                throw new NuKeeperException(
                    $"Git Cannot compare branches: the branch named '{baseBranchName}' doesn't exist");
            }
            if (!BranchExists(headBranchName))
            {
                throw new NuKeeperException(
                    $"Git Cannot compare branches: the branch named '{headBranchName}' doesn't exist");
            }

            using (var repo = MakeRepo())
            {
                var baseBranch = repo.Branches[baseBranchName];
                var headBranch = repo.Branches[headBranchName];

                var filter = new CommitFilter
                {
                    SortBy = CommitSortStrategies.Time,
                    ExcludeReachableFrom = baseBranch,
                    IncludeReachableFrom = headBranch
                };

                return repo.Commits.QueryBy(filter).Select(c => c.MessageShort).ToArray();
            }
        }
    }
}
