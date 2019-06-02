using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Update.ProcessRunner;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NuKeeper.Git
{
    public class GitCmdDiscoveryDriver : IGitDiscoveryDriver
    {
        private readonly INuKeeperLogger _logger;
        private string _pathGit;

        public GitCmdDiscoveryDriver(string pathToGit, INuKeeperLogger logger)
        {
            if (string.IsNullOrWhiteSpace(pathToGit))
            {
                throw new ArgumentNullException(nameof(pathToGit));
            }

            if (Path.GetFileNameWithoutExtension(pathToGit) != "git")
            {
                throw new InvalidOperationException($"Invalid path '{pathToGit}'. Path must point to 'git' cmd");
            }

            _logger = logger;
            _pathGit = pathToGit;
        }

        public async Task<Uri> DiscoverRepo(Uri repositoryUri)
        {
            var result = await StartGitProzess("config --get remote.origin.url", true, repositoryUri.LocalPath);
            return new Uri(result);
        }

        public async Task<string> GetCurrentHead(Uri repositoryUri)
        {
            var getBranchHead = await StartGitProzess($"symbolic-ref -q --short HEAD", true, repositoryUri.LocalPath);
            return string.IsNullOrEmpty(getBranchHead) ?
                await StartGitProzess($"rev-parse HEAD", true, repositoryUri.LocalPath) :
                getBranchHead;
        }

        public Task<GitRemote> GetRemoteForPlatform(Uri repositoryUri, string platformHost)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<GitRemote>> GetRemotes(Uri repositoryUri)
        {
            var result = await StartGitProzess("remote -v", true);
            return null;
        }

        public async Task<bool> IsGitRepo(Uri repositoryUri)
        {
            var discovered = await DiscoverRepo(repositoryUri);
            if (discovered == null)
            {
                return false;
            }

            return true;
        }

        internal async Task<string> StartGitProzess(string arguments, bool ensureSuccess, string workingFolder = "")
        {
            var process = new ExternalProcess(_logger);
            var output = await process.Run(workingFolder, _pathGit, arguments, ensureSuccess);
            return output.Output.TrimEnd(Environment.NewLine.ToCharArray());
        }

    }
}
