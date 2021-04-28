using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NuKeeper.Integration.Tests.Engine
{
    [TestFixture]
    public class RepositoryFilterTests : TestWithFailureLogging
    {
        [Test]
        [Ignore("Public GIT token is expired, the project owner needs to create a new key")]
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
        [Ignore("Public GIT token is expired, the project owner needs to create a new key")]
        public async Task ShouldNotFilterOutADotnetRepository()
        {
            IRepositoryFilter subject = MakeRepositoryFilter();

            var result =
                await subject.ContainsDotNetProjects(new RepositorySettings { RepositoryName = "sdk", RepositoryOwner = "dotnet" });
            Assert.True(result);
        }

        private RepositoryFilter MakeRepositoryFilter()
        {
            const string testKeyWithOnlyPublicAccess = "c13d2ce7774d39ae99ddaad46bd69c3d459b9992";

            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            var gitHubClient = new OctokitClient(NukeeperLogger);
            gitHubClient.Initialise(new AuthSettings(new Uri("https://api.github.com"), testKeyWithOnlyPublicAccess));
            collaborationFactory.CollaborationPlatform.Returns(gitHubClient);

            return new RepositoryFilter(collaborationFactory, NukeeperLogger);
        }
    }
}
