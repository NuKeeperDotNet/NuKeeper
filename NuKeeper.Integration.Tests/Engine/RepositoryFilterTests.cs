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
                await subject.ContainsDotNetProjects(new RepositorySettings { RepositoryName = "sdk", RepositoryOwner = "dotnet" });
            Assert.True(result);
        }

        private RepositoryFilter MakeRepositoryFilter()
        {            
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            var gitHubClient = new OctokitClient(NukeeperLogger);
            gitHubClient.Initialise(new AuthSettings(new Uri("https://api.github.com"), ""));
            collaborationFactory.CollaborationPlatform.Returns(gitHubClient);

            return new RepositoryFilter(collaborationFactory, NukeeperLogger);
        }
    }
}
