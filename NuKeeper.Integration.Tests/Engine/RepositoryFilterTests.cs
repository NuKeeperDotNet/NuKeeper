using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstract;
using NuKeeper.Abstract.Configuration;
using NuKeeper.Abstract.Engine;
using NuKeeper.Github.Engine;
using NuKeeper.Github.Mappings;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Engine
{
    [TestFixture]
    public class RepositoryFilterTests
    {
        [Test]
        public async Task ShouldFilterOutNonDotnetRepository()
        {
            IRepositoryFilter subject = MakeRepositoryFilter();

            var result =
                await subject.ContainsDotNetProjects(new RepositorySettings("jquery", "jquery"));
            Assert.False(result);
        }

        [Test]
        public async Task ShouldNotFilterOutADotnetRepository()
        {
            IRepositoryFilter subject = MakeRepositoryFilter();

            var result =  await subject.ContainsDotNetProjects(new RepositorySettings("dotnet", "cli"));
            Assert.True(result);
        }

        private static IRepositoryFilter MakeRepositoryFilter()
        {
            const string testKeyWithOnlyPublicAccess = "c13d2ce7774d39ae99ddaad46bd69c3d459b9992";
            var logger = Substitute.For<INuKeeperLogger>();

            var gitHubClient = new OctokitClient(logger);
            gitHubClient.Initialise(new AuthSettings(new Uri("https://api.github.com"), testKeyWithOnlyPublicAccess));

            return new GithubRepositoryFilter(gitHubClient, logger);
        }
    }
}
