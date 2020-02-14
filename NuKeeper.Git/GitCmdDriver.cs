using System;
using System.IO;
using System.Threading.Tasks;
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

            if (Path.GetFileNameWithoutExtension(pathToGit) != "git")
            {
                throw new InvalidOperationException($"Invalid path '{pathToGit}'. Path must point to 'git' cmd");
            }

            _pathGit = pathToGit;
            _logger = logger;
            WorkingFolder = workingFolder ?? throw new ArgumentNullException(nameof(workingFolder));
            _gitCredentials = credentials ?? throw new ArgumentNullException(nameof(credentials));
        }

        public IFolder WorkingFolder { get; }

        public async Task AddRemote(string name, Uri endpoint)
        {
            await StartGitProzess($"remote add {name} {CreateCredentialsUri(endpoint, _gitCredentials)}", true);
        }

        public async Task Checkout(string branchName)
        {
            await StartGitProzess($"checkout -b {branchName} origin/{branchName}", false);
        }

        public async Task CheckoutNewBranch(string branchName)
        {
            await StartGitProzess($"checkout -b {branchName}", true);
        }

        public async Task Clone(Uri pullEndpoint)
        {
            await Clone(pullEndpoint, null);
        }

        public async Task Clone(Uri pullEndpoint, string branchName)
        {
            _logger.Normal($"Git clone {pullEndpoint}, branch {branchName ?? "default"}, to {WorkingFolder.FullPath}");
            var branchparam = branchName == null ? "" : $" -b {branchName}";
            await StartGitProzess($"clone{branchparam} {CreateCredentialsUri(pullEndpoint, _gitCredentials)} .", true); // Clone into current folder
            _logger.Detailed("Git clone complete");
        }

        public async Task Commit(string message)
        {
            _logger.Detailed($"Git commit with message '{message}'");
            await StartGitProzess($"commit -a -m \"{message}\"", true);
        }

        public async Task<string> GetCurrentHead()
        {
            var getBranchHead = await StartGitProzess($"symbolic-ref -q --short HEAD", true);
            return string.IsNullOrEmpty(getBranchHead) ?
                await StartGitProzess($"rev-parse HEAD", true) :
                getBranchHead;
        }

        public async Task Push(string remoteName, string branchName)
        {
            _logger.Detailed($"Git push to {remoteName}/{branchName}");
            await StartGitProzess($"push {remoteName} {branchName}", true);
        }


        private  async Task<string> StartGitProzess(string arguments, bool ensureSuccess)
        {
            var process = new ExternalProcess(_logger);
            var output = await process.Run(WorkingFolder.FullPath, _pathGit, arguments, ensureSuccess);
            return output.Output.TrimEnd(Environment.NewLine.ToCharArray());
        }

        private Uri CreateCredentialsUri(Uri pullEndpoint, GitUsernamePasswordCredentials gitCredentials)
        {
            if (_gitCredentials == null)
            {
                return pullEndpoint;
            }

            return new UriBuilder(pullEndpoint) { UserName = Uri.EscapeDataString(gitCredentials.Username), Password = gitCredentials.Password }.Uri;
        }
    }
}
