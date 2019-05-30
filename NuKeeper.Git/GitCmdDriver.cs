using System;
using System.IO;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Git
{
    public class GitCmdDriver : IGitDriver
    {

        private GitUsernamePasswordCredentials _gitCredentials;
        private string _pathGit;
        private INuKeeperLogger _logger;

        public GitCmdDriver(string pathToGit, INuKeeperLogger logger,
            IFolder workingFolder, GitUsernamePasswordCredentials credentials)
        {
            if (string.IsNullOrWhiteSpace(pathToGit))
            {
                throw new ArgumentNullException(nameof(pathToGit));
            }

            if (Path.GetFileName(pathToGit) != "git.exe")
            {
                throw new InvalidOperationException($"Invalid path '{pathToGit}'. Path must point to 'git.exe'");
            }

            if (workingFolder == null)
            {
                throw new ArgumentNullException(nameof(workingFolder));
            }

            if (credentials == null)
            {
                throw new ArgumentNullException(nameof(credentials));
            }

            _pathGit = pathToGit;
            _logger = logger;
            WorkingFolder = workingFolder;
            _gitCredentials = credentials;
        }

        public IFolder WorkingFolder { get; }

        public void AddRemote(string name, Uri endpoint)
        {
            StartGitProzess($"remote add {name} {CreateCredentialsUri(endpoint, _gitCredentials)}", true);
        }

        public void Checkout(string branchName)
        {
            StartGitProzess($"checkout -b {branchName} origin/{branchName}", false);
        }

        public void CheckoutNewBranch(string branchName)
        {
            StartGitProzess($"checkout -b {branchName}", true);
        }

        public void Clone(Uri pullEndpoint)
        {
            Clone(pullEndpoint, null);
        }

        public void Clone(Uri pullEndpoint, string branchName)
        {
            _logger.Normal($"Git clone {pullEndpoint}, branch {branchName ?? "default"}, to {WorkingFolder.FullPath}");
            var branchparam = branchName == null ? "" : $" -b {branchName}";
            StartGitProzess($"clone{branchparam} {CreateCredentialsUri(pullEndpoint, _gitCredentials)} .", true); // Clone into current folder
            _logger.Detailed("Git clone complete");
        }

        public void Commit(string message)
        {
            _logger.Detailed($"Git commit with message '{message}'");
            StartGitProzess($"commit -a -m \"{message}\"", true);
        }

        public string GetCurrentHead()
        {
            var getBranchHead = StartGitProzess($"symbolic-ref -q --short HEAD", true);
            return string.IsNullOrEmpty(getBranchHead) ?
                StartGitProzess($"rev-parse HEAD", true) :
                getBranchHead;
        }

        public void Push(string remoteName, string branchName)
        {
            _logger.Detailed($"Git push to {remoteName}/{branchName}");
            StartGitProzess($"push {remoteName} {branchName}", true);
        }


        private string StartGitProzess(string arguments, bool ensureSuccess)
        {
            var process = new ExternalProcess(_logger);
            var output = process.Run(WorkingFolder.FullPath, _pathGit, arguments, ensureSuccess).Result;
            return output.Output.TrimEnd(Environment.NewLine.ToCharArray());
        }

        private Uri CreateCredentialsUri(Uri pullEndpoint, GitUsernamePasswordCredentials gitCredentials)
        {
            if (_gitCredentials == null)
            {
                return pullEndpoint;
            }

            return new UriBuilder(pullEndpoint) { UserName = gitCredentials.Username, Password = gitCredentials.Password }.Uri;
        }
    }
}
