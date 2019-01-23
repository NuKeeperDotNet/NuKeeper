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

        public async Task Clone(Uri pullEndpoint)
        {
            _logger.Normal($"Git clone {pullEndpoint} to {WorkingFolder.FullPath}");

            var gitProcess = new ExternalProcess(_logger);
            await gitProcess.Run(
                WorkingFolder.FullPath,
                await _gitPath.Executable,
                ArgumentEscaper.EscapeAndConcatenate(
                    new[]
                    {
                        "clone", "--config", "credential.helper=cache --timeout=28800", "--depth", "1",
                        pullEndpoint.AbsoluteUri, "."
                    }),
                new[] {_credentials.Username, _credentials.Password},
                true);

            _logger.Detailed("Git clone complete");
        }

        public void AddRemote(string name, Uri endpoint)
        {
            throw new NotImplementedException();
        }

        public void Checkout(string branchName)
        {
            throw new NotImplementedException();
        }

        public void CheckoutNewBranch(string branchName)
        {
            throw new NotImplementedException();
        }

        public void Commit(string message)
        {
            throw new NotImplementedException();
        }

        public async Task Push(string remoteName, string branchName)
        {
            _logger.Detailed($"Git push to {remoteName}/{branchName}");

            var gitProcess = new ExternalProcess(_logger);
            await gitProcess.Run(
                WorkingFolder.FullPath,
                await _gitPath.Executable,
                ArgumentEscaper.EscapeAndConcatenate(
                    new[]
                    {
                        "push", "--porcelain", remoteName, branchName
                    }),
                null,
                true);
        }

        public string GetCurrentHead()
        {
            throw new NotImplementedException();
        }
    }
}
