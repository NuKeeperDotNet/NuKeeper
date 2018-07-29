using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class ForkFinderTests
    {
        [Test]
        public async Task ThrowsWhenNoPushableForkCanBeFound()
        {
            var fallbackFork = DefaultFork();

            var forkFinder = new ForkFinder(Substitute.For<IGitHub>(),
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        [Test]
        public async Task FallbackForkIsUsedWhenItIsFound()
        {
            var fallbackFork = DefaultFork();
            var fallbackRepoData = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(fallbackRepoData);

            var forkFinder = new ForkFinder(github, 
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.Null);
            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task FallbackForkIsNotUsedWhenItIsNotPushable()
        {
            var fallbackFork = DefaultFork();
            var fallbackRepoData = RepositoryBuilder.MakeRepository(true, false);

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(fallbackRepoData);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        [Test]
        public async Task WhenSuitableUserForkIsFoundItIsUsedOverFallback()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task WhenSuitableUserForkIsFound_ThatMatchesParentHtmlUrl_ItIsUsedOverFallback()
        {
            var fallbackFork = new ForkData(new Uri(RepositoryBuilder.ParentHtmlUrl), "testOrg", "someRepo");

            var userRepo = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task WhenUnsuitableUserForkIsFoundItIsNotUsed()
        {
            var fallbackFork = NoMatchFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task WhenUserForkIsNotFoundItIsCreated()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns((IRepository)null);
            github.MakeUserFork(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), Substitute.For<INuKeeperLogger>());

            var actualFork = await forkFinder.FindPushFork("testUser", fallbackFork);

            await github.Received(1).MakeUserFork(Arg.Any<string>(), Arg.Any<string>());

            Assert.That(actualFork, Is.Not.Null);
            Assert.That(actualFork, Is.Not.EqualTo(fallbackFork));
        }

        [Test]
        public async Task PreferSingleRepoModeWillNotPreferFork()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferSingleRepoSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task PreferSingleRepoModeWillUseForkWhenUpstreamIsUnsuitable()
        {
            var fallbackFork = DefaultFork();

            var github = Substitute.For<IGitHub>();

            var defaultRepo = RepositoryBuilder.MakeRepository(true, false);
            github.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(defaultRepo);

            var userRepo = RepositoryBuilder.MakeRepository();

            github.GetUserRepository("testUser", fallbackFork.Name)
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferSingleRepoSettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task SingleRepoOnlyModeWillNotPreferFork()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RepositoryBuilder.MakeRepository();

            var github = Substitute.For<IGitHub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakeSingleRepoOnlySettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }


        [Test]
        public async Task SingleRepoOnlyModeWillNotUseForkWhenUpstreamIsUnsuitable()
        {
            var fallbackFork = DefaultFork();

            var github = Substitute.For<IGitHub>();

            var defaultRepo = RepositoryBuilder.MakeRepository(true, false);
            github.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(defaultRepo);

            var userRepo = RepositoryBuilder.MakeRepository();

            github.GetUserRepository("testUser", fallbackFork.Name)
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakeSingleRepoOnlySettings(), Substitute.For<INuKeeperLogger>());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Null);
        }

        private ForkData DefaultFork()
        {
            return new ForkData(new Uri(RepositoryBuilder.ParentCloneUrl), "testOrg", "someRepo");
        }

        private ForkData NoMatchFork()
        {
            return new ForkData(new Uri(RepositoryBuilder.NoMatchUrl), "testOrg", "someRepo");
        }

        private UserSettings MakePreferForkSettings()
        {
            return new UserSettings
            {
                ForkMode = ForkMode.PreferFork
            };
        }

        private UserSettings MakePreferSingleRepoSettings()
        {
            return new UserSettings
            {
                ForkMode = ForkMode.PreferSingleRepository
            };
        }

        private UserSettings MakeSingleRepoOnlySettings()
        {
            return new UserSettings
            {
                ForkMode = ForkMode.SingleRepositoryOnly
            };
        }

        private static void AssertForkMatchesRepo(ForkData fork, IRepository repo)
        {
            Assert.That(fork, Is.Not.Null);
            Assert.That(fork.Name, Is.EqualTo(repo.Name));
            Assert.That(fork.Owner, Is.EqualTo(repo.Owner.Login));
            Assert.That(fork.Uri, Is.EqualTo(new Uri(repo.CloneUrl)));
        }
    }
}
