﻿using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NUnit.Framework;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class ForkFinderTests
    {
        [Test]
        public void ThrowsWhenNoPushableForkCanBeFound()
        {
            var fallbackFork = DefaultFork();

            var forkFinder = new ForkFinder(Substitute.For<IGithub>(),
                MakePreferForkSettings(), new NullNuKeeperLogger());

            Assert.ThrowsAsync<Exception>(async () =>
                await forkFinder.FindPushFork("testUser", fallbackFork));
        }

        [Test]
        public async Task FallbackForkIsUsedWhenItIsFound()
        {
            var fallbackFork = DefaultFork();

            var github = Substitute.For<IGithub>();
            var defaultRepo = RespositoryBuilder.MakeRepository();
            github.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(defaultRepo);

            var forkFinder = new ForkFinder(github, 
                MakePreferForkSettings(), new NullNuKeeperLogger());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.Null);
            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public void FallbackForkIsNotUsedWhenItIsNotPushable()
        {
            var fallbackFork = DefaultFork();

            var github = Substitute.For<IGithub>();
            var defaultRepo = RespositoryBuilder.MakeRepository("http://a.com", true, false);
            github.GetUserRepository(fallbackFork.Owner, fallbackFork.Name)
                .Returns(defaultRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), new NullNuKeeperLogger());

            Assert.ThrowsAsync<Exception>(async () =>
                await forkFinder.FindPushFork("testUser", fallbackFork));
        }

        [Test]
        public async Task WhenSuitableUserForkIsFoundItIsUsedOverUpstream()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RespositoryBuilder.MakeRepository();

            var github = Substitute.For<IGithub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), new NullNuKeeperLogger());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.Not.EqualTo(fallbackFork));
            AssertForkMatchesRepo(fork, userRepo);
        }

        [Test]
        public async Task WhenUnsuitableUserForkIsFoundItIsNotUsed()
        {
            var fallbackFork = NoMatchFork();

            var userRepo = RespositoryBuilder.MakeRepository();

            var github = Substitute.For<IGithub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), new NullNuKeeperLogger());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }

        [Test]
        public async Task WhenUserForkIsNotFoundItIsCreated()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RespositoryBuilder.MakeRepository();

            var github = Substitute.For<IGithub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns((Repository)null);
            github.MakeUserFork(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferForkSettings(), new NullNuKeeperLogger());

            var actualFork = await forkFinder.FindPushFork("testUser", fallbackFork);

            await github.Received(1).MakeUserFork(Arg.Any<string>(), Arg.Any<string>());

            Assert.That(actualFork, Is.Not.Null);
            Assert.That(actualFork, Is.Not.EqualTo(fallbackFork));
        }

        [Test]
        public async Task PreferUpstreamModeWillNotPreferFork()
        {
            var fallbackFork = DefaultFork();

            var userRepo = RespositoryBuilder.MakeRepository();

            var github = Substitute.For<IGithub>();
            github.GetUserRepository(Arg.Any<string>(), Arg.Any<string>())
                .Returns(userRepo);

            var forkFinder = new ForkFinder(github,
                MakePreferUpstreamSettings(), new NullNuKeeperLogger());

            var fork = await forkFinder.FindPushFork("testUser", fallbackFork);

            Assert.That(fork, Is.EqualTo(fallbackFork));
        }


        private ForkData DefaultFork()
        {
            return new ForkData(new Uri(RespositoryBuilder.ParentUrl), "testOrg", "someRepo");
        }

        private ForkData NoMatchFork()
        {
            return new ForkData(new Uri(RespositoryBuilder.NoMatchUrl), "testOrg", "someRepo");
        }

        private UserSettings MakePreferForkSettings()
        {
            return new UserSettings
            {
                ForkMode = ForkMode.PreferFork
            };
        }

        private UserSettings MakePreferUpstreamSettings()
        {
            return new UserSettings
            {
                ForkMode = ForkMode.PreferUpstream
            };
        }

        private static void AssertForkMatchesRepo(ForkData fork, Repository repo)
        {
            Assert.That(fork, Is.Not.Null);
            Assert.That(fork.Name, Is.EqualTo(repo.Name));
            Assert.That(fork.Owner, Is.EqualTo(repo.Owner.Login));
            Assert.That(fork.Uri, Is.EqualTo(new Uri(repo.HtmlUrl)));
        }
    }
}
