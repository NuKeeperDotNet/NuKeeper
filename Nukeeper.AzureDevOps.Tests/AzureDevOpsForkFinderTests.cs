using System;
using NSubstitute;
using NUnit.Framework;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.AzureDevOps;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.Configuration;

namespace Nukeeper.AzureDevOps.Tests
{
    public class AzureDevOpsForkFinderTests
    {

        [Test]
        public async Task ThrowsWhenNoPushableForkCanBeFound()
        {
            var fallbackFork = DefaultFork();

            var forkFinder = new AzureDevOpsForkFinder(Substitute.For<ICollaborationPlatform>(), Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        [Test]
        public void ThrowsWhenPreferFork()
        {
            var fallbackFork = DefaultFork();

            var argument = Assert.Throws<ArgumentOutOfRangeException>(() => new AzureDevOpsForkFinder(Substitute.For<ICollaborationPlatform>(), Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork));
        }


        [Test]
        public void ThrowsWhenPreferSingleRepository()
        {
            var fallbackFork = DefaultFork();

            var argument = Assert.Throws<ArgumentOutOfRangeException>(() => new AzureDevOpsForkFinder(Substitute.For<ICollaborationPlatform>(), Substitute.For<INuKeeperLogger>(), ForkMode.PreferSingleRepository));
        }

        [Test]
        public async Task FallbackForkIsUsedWhenItIsFound()
        {
            var fallbackFork = DefaultFork();
            var fallbackRepoData = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(fallbackRepoData);

            var forkFinder = new AzureDevOpsForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.Null);
            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task FallbackForkIsNotUsedWhenItIsNotPushable()
        {
            var fallbackFork = DefaultFork();
            var fallbackRepoData = RepositoryBuilder.MakeRepository(true, false);

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(fallbackRepoData);

            var forkFinder = new AzureDevOpsForkFinder(Substitute.For<ICollaborationPlatform>(), Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        [Test]
        public async Task SingleRepoOnlyModeWillNotPreferFork()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new AzureDevOpsForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }


        [Test]
        public async Task SingleRepoOnlyModeWillNotUseForkWhenUpstreamIsUnsuitable()
        {
            var fallbackFork = DefaultFork();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();

            var defaultRepo = RepositoryBuilder.MakeRepository(true, false);
            collaborationPlatform.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(defaultRepo);

            var userRepo = RepositoryBuilder.MakeRepository();

            collaborationPlatform.GetUserRepository("testUser", fallbackFork.Name)
                .Returns(userRepo);

            var forkFinder = new AzureDevOpsForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        private static ForkData DefaultFork()
        {
            return new ForkData(RepositoryBuilder.ParentCloneUrl, "testOrg", "someRepo");
        }
    }
}
