using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.ProcessRunner;

namespace NuKeeper.Git
{
    internal class GitCommandLineDriver : IGitDriver
    {
        private readonly GitCommandlinePath _gitPath;
        private readonly INuKeeperLogger _logger;
        private readonly GitUsernamePasswordCredentials _credentials;
        private readonly User _user;

        public GitCommandLineDriver(INuKeeperLogger logger, IFolder workingFolder,
            GitUsernamePasswordCredentials credentials, User user, GitCommandlinePath gitPath)
        {
            _logger = logger;
            _credentials = credentials;
            _user = user;
            _gitPath = gitPath;
            WorkingFolder = workingFolder;
        }

        public IFolder WorkingFolder { get; }

        public Task Clone(Uri pullEndpoint)
        {
            throw new NotImplementedException();
        }

        public void AddRemote(string name, Uri endpoint)
        {
            throw new NotImplementedException();
        }

        public async Task Checkout(string branchName)
        {
            _logger.Detailed($"Git checkout '{branchName}'");
            var gitProcess = new ExternalProcess(_logger);

            await gitProcess.Run(
                WorkingFolder.FullPath,
                await _gitPath.Executable,
                ArgumentEscaper.EscapeAndConcatenate(
                    new[]
                    {
                        "checkout", branchName
                    }),
                true);
        }

        public void CheckoutNewBranch(string branchName)
        {
            throw new NotImplementedException();
        }

        public void Commit(string message)
        {
            throw new NotImplementedException();
        }

        public void Push(string remoteName, string branchName)
        {
            throw new NotImplementedException();
        }

        public string GetCurrentHead()
        {
            throw new NotImplementedException();
        }
    }
}
