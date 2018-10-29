using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Engine;
using NuKeeper.Github.Engine;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class RepositoryFilterTests
    {
        [Test]
        public async Task ShouldFilterWhenNoMatchFound()
        {
            var githubClient = Substitute.For<IClient>();
            var searchCodeResult = new SearchCodeResult(0);
            
            githubClient.Search(null).ReturnsForAnyArgs(searchCodeResult);

            IRepositoryFilter subject = new GithubRepositoryFilter(githubClient, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.False(result);
        }

        [Test]
        public async Task ShouldNotFilterWhenMatchFound()
        {
            var githubClient = Substitute.For<IClient>();
            var searchCodeResult = new SearchCodeResult(1);

            githubClient.Search(null).ReturnsForAnyArgs(searchCodeResult);

            IRepositoryFilter subject = new GithubRepositoryFilter(githubClient, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.True(result);
        }

        [Test]
        public async Task ShouldNotFilterWhenSearchFails()
        {
            var githubClient = Substitute.For<IClient>();
            githubClient.Search(null).ThrowsForAnyArgs(new Exception());

            IRepositoryFilter subject = new GithubRepositoryFilter(githubClient, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.True(result);
        }

        private static RepositorySettings MakeSampleRepository() => new RepositorySettings("sample-owner", "sample-repo"); 
    }
}
