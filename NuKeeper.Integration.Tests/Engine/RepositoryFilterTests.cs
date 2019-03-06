using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
using NuKeeper.GitHub;
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
                await subject.ContainsDotNetProjects(new RepositorySettings
                {
                    RepositoryName = "jquery",
                    RepositoryOwner = "jquery"
                });
            Assert.False(result);
        }

        [Test]
        public async Task ShouldNotFilterOutADotnetRepository()
        {
            IRepositoryFilter subject = MakeRepositoryFilter();

            var result =
                await subject.ContainsDotNetProjects(new RepositorySettings { RepositoryName = "cli", RepositoryOwner = "dotnet" });
            Assert.True(result);
        }

        private static RepositoryFilter MakeRepositoryFilter()
        {
            const string testKeyWithOnlyPublicAccess = "c13d2ce7774d39ae99ddaad46bd69c3d459b9992";
            var logger = Substitute.For<INuKeeperLogger>();

            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            var gitHubClient = new OctokitClient(logger);
            gitHubClient.Initialise(new AuthSettings(new Uri("https://api.github.com"), testKeyWithOnlyPublicAccess));
            collaborationFactory.CollaborationPlatform.Returns(gitHubClient);

            return new RepositoryFilter(collaborationFactory, logger);
        }
    }
}
