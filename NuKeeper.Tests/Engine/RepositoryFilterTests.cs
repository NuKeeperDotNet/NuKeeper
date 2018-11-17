using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class RepositoryFilterTests
    {
        [Test]
        public async Task ShouldFilterWhenNoMatchFound()
        {
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.CollaborationPlatform.Search(null).ReturnsForAnyArgs(Task.FromResult(new SearchCodeResult(0)));

            IRepositoryFilter subject = new RepositoryFilter(collaborationFactory, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.False(result);
        }

        [Test]
        public async Task ShouldNotFilterWhenMatchFound()
        {
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.CollaborationPlatform.Search(null).ReturnsForAnyArgs(Task.FromResult(new SearchCodeResult(1)));

            IRepositoryFilter subject = new RepositoryFilter(collaborationFactory, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.True(result);
        }

        [Test]
        public async Task ShouldNotFilterWhenSearchFails()
        {
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.CollaborationPlatform.Search(null).ThrowsForAnyArgs(new Exception());

            IRepositoryFilter subject = new RepositoryFilter(collaborationFactory, Substitute.For<INuKeeperLogger>());

            var result = await subject.ContainsDotNetProjects(MakeSampleRepository());

            Assert.False(result);
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
