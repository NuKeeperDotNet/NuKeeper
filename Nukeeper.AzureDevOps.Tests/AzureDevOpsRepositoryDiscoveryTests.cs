using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NUnit.Framework;

namespace Nukeeper.AzureDevOps.Tests
{
    public class AzureDevOpsRepositoryDiscoveryTests
    {
        [Test]
        public async Task SuccessInRepoMode()
        {
            var settings = new SourceControlServerSettings
            {
                Repository = new RepositorySettings { RepositoryUri = new Uri("https://repo/") },
                Scope = ServerScope.Repository
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));
            Assert.That(repos[0], Is.EqualTo(settings.Repository));
        }

        [Test]
        public async Task SuccessInRepoModeReplacesToken()
        {
            var settings = new SourceControlServerSettings
            {
                Repository = new RepositorySettings { RepositoryUri = new Uri("https://user:--PasswordToReplace--@repo/") },
                Scope = ServerScope.Repository
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));
            Assert.That(repos[0], Is.EqualTo(settings.Repository));
            Assert.That(repos[0].RepositoryUri.ToString(), Is.EqualTo("https://user:token@repo/"));
        }

        [Test]
        public async Task RepoModeIgnoresIncludesAndExcludes()
        {
            var settings = new SourceControlServerSettings
            {
                Repository = new RepositorySettings(RepositoryBuilder.MakeRepository(name: "foo")),
                Scope = ServerScope.Repository,
                IncludeRepos = new Regex("^foo"),
                ExcludeRepos = new Regex("^foo")
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo("foo"));
        }

        [Test]
        public void FailureInOrgMode()
        {
            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();
            Assert.Throws<NotImplementedException>(() => githubRepositoryDiscovery.GetRepositories(new SourceControlServerSettings { Scope = ServerScope.Organisation }));
        }

        [Test]
        public void FailureInGlobalMode()
        {
            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();
            Assert.Throws<NotImplementedException>(() => githubRepositoryDiscovery.GetRepositories(new SourceControlServerSettings { Scope = ServerScope.Global }));
        }

        private static IRepositoryDiscovery MakeGithubRepositoryDiscovery()
        {
            return new AzureDevOpsRepositoryDiscovery(Substitute.For<INuKeeperLogger>(), "token");
        }
    }
}
