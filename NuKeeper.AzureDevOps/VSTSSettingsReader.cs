using System;
using System.Linq;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.AzureDevOps
{
    public class VstsSettingsReader : BaseSettingsReader
    {
        private readonly IGitDiscoveryDriver _gitDriver;

        public VstsSettingsReader(IGitDiscoveryDriver gitDriver)
        {
            _gitDriver = gitDriver;
        }

        public override bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return false;
            }

            // Is the specified folder already a git repository?
            if (repositoryUri.IsFile)
            {
                if (_gitDriver.IsGitRepo(repositoryUri))
                {
                    // Check the origin remotes
                    var remotes = _gitDriver.GetRemotes(repositoryUri);
                    var origin = remotes.FirstOrDefault(a =>
                        a.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                    if (origin != null)
                        repositoryUri = origin.Url;
                }

            }

            return repositoryUri?.Host.ContainsOrdinal("visualstudio.com") == true;
        }

        public override RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            Uri localRepositoryUri = null;
            Uri workingFolder = repositoryUri;
            string branchName = string.Empty;
            string remoteName = null;

            if (_gitDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var remotes = _gitDriver.GetRemotes(repositoryUri);
                var origin = remotes.FirstOrDefault(a =>
                    a.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                if (origin != null)
                {
                    localRepositoryUri = _gitDriver.DiscoverRepo(repositoryUri); // Set to the folder, because we found a remote git repository
                    repositoryUri = origin.Url;
                    branchName = _gitDriver.GetCurrentHead(localRepositoryUri);
                    remoteName = "origin";
                }

            }

            // URL pattern is
            // https://{org}.visualstudio.com/{project}/_git/{repo} or
            // https://{org}.visualstudio.com/_git/{repo} for the default repo
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            var org = repositoryUri.Host.Split('.')[0];
            string repoName, project;

            if (pathParts.Count == 3)
            {
                project = pathParts[0];
                repoName = pathParts[2];
            }
            else if (pathParts.Count == 2)
            {
                project = pathParts[1];
                repoName = pathParts[1];
            }
            else
            {
                return null;
            }

            return new RepositorySettings
            {
                ApiUri = new Uri($"https://{org}.visualstudio.com/"),
                RepositoryUri = new Uri($"https://{org}.visualstudio.com/{project}/_git/{repoName}/"),
                RepositoryName = repoName,
                RepositoryOwner = project,
                LocalRepositoryUri = localRepositoryUri,
                WorkingFolder = workingFolder,
                BranchName = branchName,
                RemoteName = remoteName
            };
        }
    }
}
