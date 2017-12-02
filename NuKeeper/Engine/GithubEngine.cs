﻿using System;
using System.Threading.Tasks;
using LibGit2Sharp;
using NuKeeper.Configuration;
using NuKeeper.Files;
using NuKeeper.Git;
using NuKeeper.Github;
using NuKeeper.Logging;

namespace NuKeeper.Engine
{
    public class GithubEngine
    {
        private readonly IGithubRepositoryDiscovery _repositoryDiscovery;
        private readonly IGithub _github;
        private readonly IRepositoryUpdater _repositoryUpdater;
        private readonly INuKeeperLogger _logger;
        private readonly IFolderFactory _folderFactory;
        private readonly string _githubToken;

        public GithubEngine(
            IGithubRepositoryDiscovery repositoryDiscovery, 
            IGithub github,
            IRepositoryUpdater repositoryUpdater,
            INuKeeperLogger logger,
            IFolderFactory folderFactory,
            GithubAuthSettings settings)
        {
            _repositoryDiscovery = repositoryDiscovery;
            _github = github;
            _repositoryUpdater = repositoryUpdater;
            _logger = logger;
            _folderFactory = folderFactory;
            _githubToken = settings.Token;
        }

        public async Task Run()
        {
            _folderFactory.DeleteExistingTempDirs();

            var githubUser = await _github.GetCurrentUser();
            var gitCreds = new UsernamePasswordCredentials
            {
                Username = githubUser,
                Password = _githubToken
            };

            var repositories = await _repositoryDiscovery.GetRepositories();

            foreach (var repository in repositories)
            {
                await RunRepo(repository, gitCreds);
            }
        }

        private async Task RunRepo(RepositorySettings repository, Credentials gitCreds)
        {
            try
            {
                var tempFolder = _folderFactory.UniqueTemporaryFolder();
                var git = new LibGit2SharpDriver(_logger, tempFolder, gitCreds);

                // for now we pull and push from the same place
                var oneBranchOnly = new ForkSpec(repository.GithubUri, repository.RepositoryOwner, repository.RepositoryName);
                var repo = new RepositorySpec(oneBranchOnly, oneBranchOnly);

                await _repositoryUpdater.Run(git, repo);

                tempFolder.TryDelete();
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed on repo {repository.RepositoryName}", ex);
            }
        }
    }
}
