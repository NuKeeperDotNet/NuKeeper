using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NUnit.Framework;

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

        private static IGithubRepositoryDiscovery MakeGithubRepositoryDiscovery(IGithub github, ModalSettings settings)
        {
            return new GithubRepositoryDiscovery(github, settings, new NullNuKeeperLogger());
        }
    }
}
