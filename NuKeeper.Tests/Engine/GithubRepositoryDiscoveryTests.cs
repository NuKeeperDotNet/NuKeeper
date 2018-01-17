using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NUnit.Framework;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GithubRepositoryDiscoveryTests
    {
        [Test]
        public async Task SuccessInRepoMode()
        {
            var github = Substitute.For<IGithub>();
            var settings = new ModalSettings
            {
                Mode = GithubMode.Repository,
                Repository = new RepositorySettings()
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery(github, settings);

            var reposResponse = await githubRepositoryDiscovery.GetRepositories();

            var repos = reposResponse.ToList();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count, Is.EqualTo(1));
            Assert.That(repos[0], Is.EqualTo(settings.Repository));
        }

        [Test]
        public async Task SuccessInOrgMode()
        {
            var github = Substitute.For<IGithub>();
            var settings = new ModalSettings
                {
                    Mode = GithubMode.Organisation,
                    OrganisationName = "testOrg"
                };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery(github, settings);

            var repos = await githubRepositoryDiscovery.GetRepositories();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Empty);
        }

        [Test]
        public async Task OrgModeValidReposAreIncluded()
        {
            var inputRepos = new List<Repository>
            {
                RespositoryBuilder.MakeRepository()
            };

            IReadOnlyList<Repository> readOnlyRepos = inputRepos.AsReadOnly();
            
            var github = Substitute.For<IGithub>();
            github.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var settings = new ModalSettings
            {
                Mode = GithubMode.Organisation,
                OrganisationName = "testOrg"
            };

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery(github, settings);

            var repos = await githubRepositoryDiscovery.GetRepositories();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[0].Name));
            Assert.That(firstRepo.GithubUri.ToString(), Is.EqualTo(inputRepos[0].HtmlUrl));
        }

        private static IGithubRepositoryDiscovery MakeGithubRepositoryDiscovery(IGithub github, ModalSettings settings)
        {
            return new GithubRepositoryDiscovery(github, settings, new NullNuKeeperLogger());
        }
    }
}
