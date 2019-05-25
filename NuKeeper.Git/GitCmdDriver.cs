using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NuKeeper.Git
{
    public class GitCmdDriver : IGitDriver
    {

        private GitUsernamePasswordCredentials _gitCredentials;
        private User _user;
        private string _pathGit;
        private INuKeeperLogger _logger;

        public GitCmdDriver(string pathToGit, INuKeeperLogger logger,
            IFolder workingFolder, GitUsernamePasswordCredentials credentials, User user)
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
            _user = user;
        }

        public IFolder WorkingFolder { get; }

        public void AddRemote(string name, Uri endpoint)
        {
            StartGitProzess($"remote add {name} {endpoint}");
        }

        public void Checkout(string branchName)
        {
            StartGitProzess($"checkout -b {branchName} origin/{branchName}");
        }

        public void CheckoutNewBranch(string branchName)
        {
            StartGitProzess($"checkout -b {branchName}");
        }

        public void Clone(Uri pullEndpoint)
        {
            Clone(pullEndpoint, null);
        }

        public void Clone(Uri pullEndpoint, string branchName)
        {
            _logger.Normal($"Git clone {pullEndpoint}, branch {branchName ?? "default"}, to {WorkingFolder.FullPath}");
            var branchparam = branchName == null ? "" : $" -b {branchName}";
            StartGitProzess($"clone{branchparam} {CreateCredentialsUri(pullEndpoint, _gitCredentials)} ."); // Clone into current folder
            _logger.Detailed("Git clone complete");
        }

        public void Commit(string message)
        {
            _logger.Detailed($"Git commit with message '{message}'");
            StartGitProzess($"commit -m \"{message}\"");
        }

        public string GetCurrentHead()
        {
            var getBranchHead = StartGitProzess($"symbolic-ref -q --short HEAD");
            return string.IsNullOrEmpty(getBranchHead) ?
                StartGitProzess($"rev-parse HEAD") :
                getBranchHead;
        }

        public void Push(string remoteName, string branchName)
        {
            _logger.Detailed($"Git push to {remoteName}/{branchName}");
            StartGitProzess($"push {remoteName} {branchName}");
        }


        private string StartGitProzess(string arguments)
        {
            try
            {
                ProcessStartInfo gitInfo = new ProcessStartInfo
                {
                    CreateNoWindow = false,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = _pathGit,
                    WorkingDirectory = WorkingFolder.FullPath,
                    Arguments = arguments
                };

                Process gitProcess = new Process
                {
                    StartInfo = gitInfo
                };

                gitProcess.Start();
                gitProcess.WaitForExit();

                string stderr_str = "";
                while ((stderr_str = gitProcess.StandardError.ReadLine()) != null)
                {
                    if (gitProcess.ExitCode == 0)
                    {
                        _logger.Normal($"Git {arguments}: {stderr_str}");
                    }
                    else
                    {
                        _logger.Error($"Git {arguments}: {stderr_str}");
                    }
                }

                string stdout_str = "";
                string returnValue = "";
                while ((stdout_str = gitProcess.StandardOutput.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(returnValue))
                    {
                        returnValue += "\n";
                    }

                    returnValue += stdout_str;
                    _logger.Detailed($"Git {arguments}: {stderr_str}");
                }

                gitProcess.Close();
                return returnValue;
            }
            catch (Exception ex)
            {
                _logger.Error("bla", ex);
            }

            return "";
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
