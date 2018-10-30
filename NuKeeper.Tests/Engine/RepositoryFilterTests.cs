using System;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NUnit.Framework;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class RepositoryFilterTests
    {
        [Test]
        public async Task ShouldFilterWhenNoMatchFound()
        {
            var githubClient = Substitute.For<IGitHub>();
            githubClient.Search(null).ReturnsForAnyArgs(Task.FromResult(new SearchCodeResult(0, false, null)));

            IRepositoryFilter subject = new RepositoryFilter(githubClient, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.False(result);
        }

        [Test]
        public async Task ShouldNotFilterWhenMatchFound()
        {
            var githubClient = Substitute.For<IGitHub>();
            githubClient.Search(null).ReturnsForAnyArgs(Task.FromResult(new SearchCodeResult(1, false, null)));

            IRepositoryFilter subject = new RepositoryFilter(githubClient, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.True(result);
        }

        [Test]
        public async Task ShouldNotFilterWhenSearchFails()
        {
            var githubClient = Substitute.For<IGitHub>();
            githubClient.Search(null).ThrowsForAnyArgs(new Exception());

            IRepositoryFilter subject = new RepositoryFilter(githubClient, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.True(result);
        }

        private static RepositorySettings MakeSampleRepository()
        {
            return new RepositorySettings
            {
                RepositoryName = "sample-repo",
                RepositoryOwner = "sample-owner"
            };
        }
    }
}
