using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GitHubRepositoryDiscoveryTests
    {
        [Test]
        public async Task SuccessInRepoMode()
        {
            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            var settings = new SourceControlServerSettings
            {
                Repository = new RepositorySettings(),
                Scope = ServerScope.Repository
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));
            Assert.That(repos[0], Is.EqualTo(settings.Repository));
        }

        [Test]
        public async Task RepoModeIgnoresIncludesAndExcludes()
        {
            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            var settings = new SourceControlServerSettings
            {
                Repository = new RepositorySettings(RepositoryBuilder.MakeRepository(name: "foo")),
                Scope = ServerScope.Repository,
                IncludeRepos = new Regex("^foo"),
                ExcludeRepos = new Regex("^foo")
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo("foo"));
        }

        [Test]
        public async Task SuccessInOrgMode()
        {
            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Empty);
        }

        [Test]
        public async Task OrgModeValidReposAreIncluded()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository()
            };
            IReadOnlyList<Repository> readOnlyRepos = inputRepos.AsReadOnly();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[0].Name));
            Assert.That(firstRepo.Uri.ToString(), Is.EqualTo(inputRepos[0].HtmlUrl));
        }

        [Test]
        public async Task OrgModeInvalidReposAreExcluded()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository("http://a.com/repo1", "http://a.com/repo1.git", false),
                RepositoryBuilder.MakeRepository("http://b.com/repob", "http://b.com/repob.git", true)
            };
            IReadOnlyList<Repository> readOnlyRepos = inputRepos.AsReadOnly();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[1].Name));
            Assert.That(firstRepo.Uri.ToString(), Is.EqualTo(inputRepos[1].HtmlUrl));
        }

        [Test]
        public async Task OrgModeWhenThereAreIncludes_OnlyConsiderMatches()
        {
            var inputRepos = new List<Repository>
            {
                RepositoryBuilder.MakeRepository(name:"foo"),
                RepositoryBuilder.MakeRepository(name:"bar")
            };
            IReadOnlyList<Repository> readOnlyRepos = inputRepos.AsReadOnly();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var settings = OrgModeSettings();
            settings.IncludeRepos = new Regex("^bar");
            var repos = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, settings);

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
            IReadOnlyList<Repository> readOnlyRepos = inputRepos.AsReadOnly();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var settings = OrgModeSettings();
            settings.ExcludeRepos = new Regex("^bar");
            var repos = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, settings);

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
            IReadOnlyList<Repository> readOnlyRepos = inputRepos.AsReadOnly();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var settings = OrgModeSettings();
            settings.IncludeRepos = new Regex("^bar");
            settings.ExcludeRepos = new Regex("^bar");
            var repos = await githubRepositoryDiscovery.GetRepositories(collaborationPlatform, settings);

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count(), Is.EqualTo(0));
        }

        private static IGitHubRepositoryDiscovery MakeGithubRepositoryDiscovery()
        {
            return new GitHubRepositoryDiscovery(Substitute.For<INuKeeperLogger>());
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
