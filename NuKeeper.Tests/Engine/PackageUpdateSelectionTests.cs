using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class PackageUpdateSelectionTests
    {
        [Test]
        public void WhenThereAreNoInputs_NoTargetsOut()
        {
            var updateSets = new List<PackageUpdateSet>();

            var target = OneTargetSelection();

            var results = target.SelectTargets(updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void WhenThereIsOneInput_ItIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet> { UpdateFooFromOneVersion() };

            var target = OneTargetSelection();

            var results = target.SelectTargets(updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].PackageId, Is.EqualTo("foo"));
        }

        [Test]
        public void WhenThereAreTwoInputs_MoreVersionsFirst_FirstIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateBarFromTwoVersions(),
                UpdateFooFromOneVersion()
            };

            var target = OneTargetSelection();

            var results = target.SelectTargets(updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].PackageId, Is.EqualTo("bar"));
        }

        [Test]
        public void WhenThereAreTwoInputs_MoreVersionsSecond_SecondIsTheTarget()
        {
            var updateSets = new List<PackageUpdateSet>
            {
                UpdateFooFromOneVersion(),
                UpdateBarFromTwoVersions()
            };

            var target = OneTargetSelection();

            var results = target.SelectTargets(updateSets);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].PackageId, Is.EqualTo("bar"));
        }

        private PackageUpdateSet UpdateFooFromOneVersion()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            return new PackageUpdateSet(newPackage, string.Empty, currentPackages);
        }

        private PackageUpdateSet UpdateBarFromTwoVersions()
        {
            var newPackage = LatestVersionOfPackageBar();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.1", PathToProjectOne()),
                new PackageInProject("bar", "1.2.1", PathToProjectTwo())
            };

            return new PackageUpdateSet(newPackage, string.Empty, currentPackages);
        }

        private PackageIdentity LatestVersionOfPackageFoo()
        {
            return new PackageIdentity("foo", new NuGetVersion("1.2.3"));
        }

        private PackageIdentity LatestVersionOfPackageBar()
        {
            return new PackageIdentity("bar", new NuGetVersion("2.3.4"));
        }

        private PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }

        private PackagePath PathToProjectTwo()
        {
            return new PackagePath("c_temp", "projectTwo", PackageReferenceType.PackagesConfig);
        }

        private static IPackageUpdateSelection OneTargetSelection()
        {
            const int maxPullRequests = 1;
            var settings = new Settings(new RepositoryModeSettings
            {
                MaxPullRequestsPerRepository = maxPullRequests
            });
            return new PackageUpdateSelection(settings);
        }
    }
}
