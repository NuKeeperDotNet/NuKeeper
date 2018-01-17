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

            var githubRepositoryDiscovery = new GithubRepositoryDiscovery(github, settings);

            var repos = await githubRepositoryDiscovery.GetRepositories();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos.Count(), Is.EqualTo(1));
            Assert.That(repos.First(), Is.EqualTo(settings.Repository));
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

            var githubRepositoryDiscovery = new GithubRepositoryDiscovery(github, settings);

            var repos = await githubRepositoryDiscovery.GetRepositories();

            Assert.That(repos, Is.Not.Null);
            Assert.That(repos, Is.Empty);
        }
    }
}
