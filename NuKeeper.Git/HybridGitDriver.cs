using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Git
{
    public class HybridGitDriver : IGitDriver
    {
        private readonly GitCommandLineDriver _gitCommandLineDriver;
        private readonly GitCommandlinePath _gitPath;
        private readonly LibGit2SharpDriver _libGitDriver;

        public HybridGitDriver(INuKeeperLogger logger, IFolder folder, GitUsernamePasswordCredentials credentials,
            User user)
        {
            _libGitDriver = new LibGit2SharpDriver(logger, folder, credentials, user);
            _gitPath = new GitCommandlinePath(logger);
            _gitCommandLineDriver = new GitCommandLineDriver(logger, folder, credentials, user, _gitPath);
        }

        public IFolder WorkingFolder => _libGitDriver.WorkingFolder;

        public async Task Clone(Uri pullEndpoint)
        {
            await _libGitDriver.Clone(pullEndpoint);
        }

        public void AddRemote(string name, Uri endpoint)
        {
            _libGitDriver.AddRemote(name, endpoint);
        }

        public async Task Checkout(string branchName)
        {
            if (await _gitPath.IsValid())
                await _gitCommandLineDriver.Checkout(branchName);
            else
                await _libGitDriver.Checkout(branchName);
        }

        public void CheckoutNewBranch(string branchName)
        {
            _libGitDriver.CheckoutNewBranch(branchName);
        }

        public void Commit(string message)
        {
            _libGitDriver.Commit(message);
        }

        public void Push(string remoteName, string branchName)
        {
            _libGitDriver.Push(remoteName, branchName);
        }

        public string GetCurrentHead()
        {
            return _libGitDriver.GetCurrentHead();
        }
    }
}
