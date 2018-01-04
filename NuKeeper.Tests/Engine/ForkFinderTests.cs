using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Engine;
using NuKeeper.Github;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class ForkFinderTests
    {
        [Test]
        public async Task FallbackForkIsUsedByDefault()
        {
            var forkFinder = new ForkFinder(Substitute.For<IGithub>(), new NullNuKeeperLogger());

            var fallbackFork = new ForkData(new Uri("http://someurl.com"), "testUser", "someRepo");

            var fork = await forkFinder.PushFork("testUser", "someRepo", fallbackFork);

            Assert.That(fork, Is.Not.Null);
            Assert.That(fork, Is.EqualTo(fallbackFork));
        }
    }
}
