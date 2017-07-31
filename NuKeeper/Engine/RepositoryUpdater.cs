﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Github.Models;
using NuKeeper.NuGet.Api;
using NuKeeper.NuGet.Process;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class RepositoryUpdater
    {   
        private readonly IPackageUpdatesLookup _packageLookup;
        private readonly RepositoryModeSettings _settings;
        private readonly string _tempDir;
        private readonly IGithub _github;
        private readonly IGitDriver _git;
        private readonly IPackageUpdateSelection _updateSelection;

        public RepositoryUpdater(IPackageUpdatesLookup packageLookup, 
            IGithub github,
            IGitDriver git,
            string tempDir,
            IPackageUpdateSelection updateSelection,
            RepositoryModeSettings settings)
        {
            _packageLookup = packageLookup;
            _github = github;
            _git = git;
            _tempDir = tempDir;
            _updateSelection = updateSelection;
            _settings = settings;
        }

        public async Task Run()
        {
            GitCloneToTempDir();
            var defaultBranch = _git.GetCurrentHead();

            // scan for nuget packages
            var repoScanner = new RepositoryScanner();
            var packages = repoScanner.FindAllNuGetPackages(_tempDir)
                .ToList();

            EngineReport.PackagesFound(packages);

            // look for package updates
            var updates = await _packageLookup.FindUpdatesForPackages(packages);
            EngineReport.UpdatesFound(updates);

            if (updates.Count == 0)
            {
                Console.WriteLine("No potential updates found. Well done. Exiting.");
                return;
            }

            var targetUpdates = _updateSelection.SelectTargets(updates);

            foreach (var updateSet in targetUpdates)
            {
                await UpdatePackageInProjects(updateSet, defaultBranch);
            }

            // delete the temp folder
            TempFiles.TryDelete(new DirectoryInfo(_tempDir));
            Console.WriteLine("Done");
        }

        private void GitCloneToTempDir()
        {
            Console.WriteLine($"Using temp dir: {_tempDir}");
            Console.WriteLine($"Git url: {_settings.GithubUri}");

            _git.Clone(_settings.GithubUri);

            Console.WriteLine("Git clone complete");
        }

        private async Task UpdatePackageInProjects(PackageUpdateSet updateSet, string defaultBranch)
        {
            try
            {
                EngineReport.OldVersionsToBeUpdated(updateSet);

                _git.Checkout(defaultBranch);

                // branch
                var branchName = $"nukeeper-update-{updateSet.PackageId}-to-{updateSet.NewVersion}";

                 _git.CheckoutNewBranch(branchName);

                Console.WriteLine($"Using branch '{branchName}'");

                await UpdateAllCurrentUsages(updateSet);

                Console.WriteLine("Commiting");

                var commitMessage = CommitReport.MakeCommitMessage(updateSet);
                _git.Commit(commitMessage);

                Console.WriteLine($"Pushing branch '{branchName}'");
                _git.Push("origin", branchName);

                var prTitle = CommitReport.MakePullRequestTitle(updateSet);
                await MakeGitHubPullRequest(updateSet, prTitle, branchName, defaultBranch);
                _git.Checkout(defaultBranch);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update failed: {ex.GetType().Name} {ex.Message}");
            }
        }

        private static async Task UpdateAllCurrentUsages(PackageUpdateSet updateSet)
        {
            foreach (var current in updateSet.CurrentPackages)
            {
                var updater = current.Path.PackageReferenceType == PackageReferenceType.ProjectFile
                    ? (INuGetUpdater) new NuGetUpdater()
                    : new PackagesConfigUpdater();

                await updater.UpdatePackage(updateSet.NewVersion, current);
            }
        }

        private async Task MakeGitHubPullRequest(PackageUpdateSet updates, string commitMessage, string branchName, string baseBranch)
        {
            Console.WriteLine($"Making PR on '{_settings.GithubApiBase} {_settings.RepositoryOwner} {_settings.RepositoryName}'");

            // open github PR
            var pr = new OpenPullRequestRequest
            {
                Data = new PullRequestData
                {
                    Title = commitMessage,
                    Body = CommitReport.MakeCommitDetails(updates),
                    Base = baseBranch,
                    Head = branchName
                },
                RepositoryOwner = _settings.RepositoryOwner,
                RepositoryName = _settings.RepositoryName
            };

            await _github.OpenPullRequest(pr);
        }
    }
}
