using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GitHubRepositoryDiscoveryTests
    {
        [Test]
        public async Task SuccessInRepoMode()
        {
            var github = Substitute.For<IGitHub>();
            var settings = new SourceControlServerSettings
            {
                Repository = new RepositorySettings(),
                Scope = ServerScope.Repository
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var reposResponse = await githubRepositoryDiscovery.GetRepositories(github, settings);

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));
            Assert.That(repos[0], Is.EqualTo(settings.Repository));
        }

        [Test]
        public async Task SuccessInOrgMode()
        {
            var github = Substitute.For<IGitHub>();

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(github, OrgModeSettings());

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
            
            var github = Substitute.For<IGitHub>();
            github.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(github, OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[0].Name));
            Assert.That(firstRepo.GithubUri.ToString(), Is.EqualTo(inputRepos[0].HtmlUrl));
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

            var github = Substitute.For<IGitHub>();
            github.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery();

            var repos = await githubRepositoryDiscovery.GetRepositories(github, OrgModeSettings());

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[1].Name));
            Assert.That(firstRepo.GithubUri.ToString(), Is.EqualTo(inputRepos[1].HtmlUrl));
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
