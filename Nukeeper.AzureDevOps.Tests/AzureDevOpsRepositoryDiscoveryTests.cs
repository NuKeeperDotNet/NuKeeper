using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
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

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery();

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

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery();

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

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo("foo"));
        }

        [Test]
        public async Task SuccessInOrgMode()
        {
            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Empty);
        }

        [Test]
        public async Task OrgModeInvalidReposAreExcluded()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository("http://a.com/repo1.git".ToUri(), false),
                RepositoryBuilder.MakeRepository("http://b.com/repob.git".ToUri(), true)
            };

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery(inputRepos.AsReadOnly());

            var repos = await githubRepositoryDiscovery.GetRepositories(OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[1].Name));
            Assert.That(firstRepo.RepositoryUri.ToString(), Is.EqualTo(inputRepos[1].CloneUrl));
        }

        [Test]
        public async Task OrgModeWhenThereAreIncludes_OnlyConsiderMatches()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository(name:"foo"),
                RepositoryBuilder.MakeRepository(name:"bar")
            };

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery(inputRepos.AsReadOnly());

            var settings = OrgModeSettings();
            settings.IncludeRepos = new Regex("^bar");
            var repos = await githubRepositoryDiscovery.GetRepositories(settings);

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo("bar"));
        }

        [Test]
        public async Task OrgModeWhenThereAreExcludes_OnlyConsiderNonMatching()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository(name:"foo"),
                RepositoryBuilder.MakeRepository(name:"bar")
            };

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery(inputRepos.AsReadOnly());

            var settings = OrgModeSettings();
            settings.ExcludeRepos = new Regex("^bar");
            var repos = await githubRepositoryDiscovery.GetRepositories(settings);

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo("foo"));
        }

        [Test]
        public async Task OrgModeWhenThereAreIncludesAndExcludes_OnlyConsiderMatchesButRemoveNonMatching()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository(name:"foo"),
                RepositoryBuilder.MakeRepository(name:"bar")
            };

            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery(inputRepos.AsReadOnly());

            var settings = OrgModeSettings();
            settings.IncludeRepos = new Regex("^bar");
            settings.ExcludeRepos = new Regex("^bar");
            var repos = await githubRepositoryDiscovery.GetRepositories(settings);

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task SuccessInGlobalMode()
        {
            var githubRepositoryDiscovery = MakeAzureDevOpsRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(new SourceControlServerSettings { Scope = ServerScope.Global });

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Empty);
        }

        private static IRepositoryDiscovery MakeAzureDevOpsRepositoryDiscovery(IReadOnlyList<Repository> repositories = null)
        {
            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(repositories ?? new List<Repository>());
            return new AzureDevOpsRepositoryDiscovery(Substitute.For<INuKeeperLogger>(), collaborationPlatform, "token");
        }

        private static SourceControlServerSettings OrgModeSettings()
        {
            return new SourceControlServerSettings
            {
                OrganisationName = "testOrg",
                Scope = ServerScope.Organisation
            };
        }
    }
}
