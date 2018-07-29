using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class GitHubRepositoryDiscoveryTests
    {
        [Test]
        public async Task SuccessInRepoMode()
        {
            var github = Substitute.For<IGitHub>();
            var settings = new ModalSettings
            {
                Mode = RunMode.Repository,
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
            var github = Substitute.For<IGitHub>();

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery(github, OrgModeSettings());

            var repos = await githubRepositoryDiscovery.GetRepositories();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Empty);
        }

        [Test]
        public async Task OrgModeValidReposAreIncluded()
        {
            var inputRepos = new List<IRepository>
            {
                MakeRepository(true, true)
            };
            IReadOnlyList<IRepository> readOnlyRepos = inputRepos.AsReadOnly();

            var github = Substitute.For<IGitHub>();
            github.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery(github, OrgModeSettings());

            var repos = await githubRepositoryDiscovery.GetRepositories();

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
            var inputRepos = new List<IRepository>
            {
                MakeRepository(false, true),
                MakeRepository(true, true)
            };
            IReadOnlyList<IRepository> readOnlyRepos = inputRepos.AsReadOnly();

            var github = Substitute.For<IGitHub>();
            github.GetRepositoriesForOrganisation(Arg.Any<string>())
                .Returns(Task.FromResult(readOnlyRepos));

            var githubRepositoryDiscovery = MakeGithubRepositoryDiscovery(github, OrgModeSettings());

            var repos = await githubRepositoryDiscovery.GetRepositories();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Not.Empty);
            Assert.That(repos.Count(), Is.EqualTo(1));

            var firstRepo = repos.First();
            Assert.That(firstRepo.RepositoryName, Is.EqualTo(inputRepos[1].Name));
            Assert.That(firstRepo.GithubUri.ToString(), Is.EqualTo(inputRepos[1].HtmlUrl));
        }

        private static IGitHubRepositoryDiscovery MakeGithubRepositoryDiscovery(IGitHub gitHub, ModalSettings settings)
        {
            return new GitHubRepositoryDiscovery(gitHub, settings, Substitute.For<INuKeeperLogger>());
        }

        private static ModalSettings OrgModeSettings()
        {
            return new ModalSettings
            {
                Mode = RunMode.Organisation,
                OrganisationName = "testOrg"
            };
        }

        private static IRepository MakeRepository(bool canPull, bool canPush)
        {
            var result = Substitute.For<IRepository>();
            result.HtmlUrl.Returns("http://sample/");
            result.Permissions.Pull.Returns(canPull);
            result.Permissions.Push.Returns(canPush);
            return result;
        }
    }
}
