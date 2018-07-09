using System;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
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

            var shouldSkip =
                await subject.ShouldSkip(new RepositorySettings
                {
                    RepositoryName = "jquery",
                    RepositoryOwner = "jquery"
                });
            Assert.True(shouldSkip);
        }

        [Test]
        public async Task ShouldNotFilterOutADotnetRepository()
        {
            IRepositoryFilter subject = MakeRepositoryFilter();

            var shouldSkip =
                await subject.ShouldSkip(new RepositorySettings {RepositoryName = "cli", RepositoryOwner = "dotnet"});
            Assert.False(shouldSkip);
        }

        private static RepositoryFilter MakeRepositoryFilter()
        {
            const string testKeyWithOnlyPublicAccess = "c13d2ce7774d39ae99ddaad46bd69c3d459b9992";
            var logger = Substitute.For<INuKeeperLogger>();
            return new RepositoryFilter(
                new OctokitClient(
                    new GithubAuthSettings(new Uri("https://api.github.com"),
                        testKeyWithOnlyPublicAccess), logger), logger);
        }
    }
}
