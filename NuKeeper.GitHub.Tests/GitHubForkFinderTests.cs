using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using NUnit.Framework;

namespace NuKeeper.GitHub.Tests
{
    [TestFixture]
    public class GitHubForkFinderTests
    {
        [Test]
        public async Task ThrowsWhenNoPushableForkCanBeFound()
        {
            var fallbackFork = DefaultFork();

            var forkFinder = new GitHubForkFinder(Substitute.For<ICollaborationPlatform>(), Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        [Test]
        public async Task FallbackForkIsUsedWhenItIsFound()
        {
            var fallbackFork = DefaultFork();
            var fallbackRepoData = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(fallbackRepoData);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

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

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        [Test]
        public async Task WhenSuitableUserForkIsFoundItIsUsedOverFallback()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task WhenSuitableUserForkIsFound_ThatMatchesParentHtmlUrl_ItIsUsedOverFallback()
        {
            var fallbackFork = new ForkData(new Uri(RepositoryBuilder.ParentHtmlUrl), "testOrg", "someRepo");

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task WhenUnsuitableUserForkIsFoundItIsNotUsed()
        {
            var fallbackFork = NoMatchFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task WhenUserForkIsNotFoundItIsCreated()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns((Repository) null);
            collaborationPlatform.MakeUserFork(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferFork);

            var actualFork = await forkFinder.FindPushFork("testUser", fallbackFork);

            await collaborationPlatform.Received(1).MakeUserFork(Arg.Any<string>(), Arg.Any<string>());

            Assert.That(actualFork, Is.Not.Null);
            Assert.That(actualFork, Is.Not.EqualTo(fallbackFork));
        }

        [Test]
        public async Task PreferSingleRepoModeWillNotPreferFork()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferSingleRepository);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task PreferSingleRepoModeWillUseForkWhenUpstreamIsUnsuitable()
        {
            var fallbackFork = DefaultFork();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();

            var defaultRepo = RepositoryBuilder.MakeRepository(true, false);
            collaborationPlatform.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(defaultRepo);

            var userRepo = RepositoryBuilder.MakeRepository();

            collaborationPlatform.GetUserRepository("testUser", fallbackFork.Name)
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.PreferSingleRepository);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task SingleRepoOnlyModeWillNotPreferFork()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var collaborationPlatform = Substitute.For<ICollaborationPlatform>();
            collaborationPlatform.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

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

            var forkFinder = new GitHubForkFinder(collaborationPlatform, Substitute.For<INuKeeperLogger>(), ForkMode.SingleRepositoryOnly);

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        private static ForkData DefaultFork()
        {
            return new ForkData(new Uri(RepositoryBuilder.ParentCloneUrl), "testOrg", "someRepo");
        }

        private static ForkData NoMatchFork()
        {
            return new ForkData(new Uri(RepositoryBuilder.NoMatchUrl), "testOrg", "someRepo");
        }

        private static void AssertForkMatchesRepo(ForkData fork, Repository repo)
        {
            Assert.That(fork, Is.Not.Null);
            Assert.That(fork.Name, Is.EqualTo(repo.Name));
            Assert.That(fork.Owner, Is.EqualTo(repo.Owner.Login));
            Assert.That(fork.Uri, Is.EqualTo(repo.CloneUrl));
        }
    }
}
