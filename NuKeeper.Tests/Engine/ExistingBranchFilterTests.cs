using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Engine.Packages;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class ExistingBranchFilterTests
    {
        [Test]
        public async Task IfBranchDoesNotExistAllowCreation()
        {
            var fork = new ForkData(new Uri("http://uri.com"), "owner", "name");
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.CollaborationPlatform.RepositoryBranchExists(fork.Owner, fork.Name, Arg.Any<string>()).Returns(false);

            var filter = new ExistingBranchFilter(collaborationFactory, Substitute.For<INuKeeperLogger>());
            var result = await filter.CanMakeBranchFor(MakeUpdateSet(), fork);
            Assert.AreEqual(result, true);
        }

        [Test]
        public async Task IfBranchDoesExistDoNotAllowCreation()
        {
            var fork = new ForkData(new Uri("http://uri.com"), "owner", "name");
            var collaborationFactory = Substitute.For<ICollaborationFactory>();
            collaborationFactory.CollaborationPlatform.RepositoryBranchExists(fork.Owner, fork.Name, Arg.Any<string>()).Returns(true);

            var filter = new ExistingBranchFilter(collaborationFactory, Substitute.For<INuKeeperLogger>());
            var result = await filter.CanMakeBranchFor(MakeUpdateSet(), fork);
            Assert.AreEqual(result, false);
        }

        private static PackageUpdateSet MakeUpdateSet()
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("id", "1.0.0", new PackagePath("base","rel", PackageReferenceType.ProjectFile)),
                new PackageInProject("id", "1.0.0", new PackagePath("base","rel", PackageReferenceType.ProjectFile)),
            };

            var majorUpdate = Metadata("id", "1.0.0", null);

            var lookupResult = new PackageLookupResult(VersionChange.Major,
                majorUpdate, null, null);
            var updates = new PackageUpdateSet(lookupResult, currentPackages);

            return updates;
        }

        private static PackageSearchMetadata Metadata(string packageId, string version, PackageDependency upstream)
        {
            var upstreams = new List<PackageDependency>();
            if (upstream != null)
            {
                upstreams.Add(upstream);
            }

            return new PackageSearchMetadata(
                new PackageIdentity(packageId, new NuGetVersion(version)),
                new PackageSource(NuGetConstants.V3FeedUrl),
                DateTimeOffset.Now, upstreams);
        }
    }
}
