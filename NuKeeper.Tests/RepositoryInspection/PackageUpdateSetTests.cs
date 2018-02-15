using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class PackageUpdateSetTests
    {
        private const string ASource = "somePackageSource";

        [Test]
        public void NullPackageMatch_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), null, packages));

            Assert.That(exception.ParamName, Is.EqualTo("match"));
        }

        [Test]
        public void NullPackageHighest_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var exception = Assert.Throws<ArgumentNullException>(() =>
                new PackageUpdateSet(VersionChange.Major, null, LatestFooMetadata(), packages));

            Assert.That(exception.ParamName, Is.EqualTo("highest"));
        }

        [Test]
        public void NullPackages_IsNotAllowed()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), null));

            Assert.That(exception.ParamName, Is.EqualTo("currentPackages"));
        }

        [Test]
        public void EmptyPackages_IsNotAllowed()
        {
            var exception = Assert.Throws<ArgumentException>(() => new PackageUpdateSet(VersionChange.Major,
                LatestFooMetadata(), LatestFooMetadata(), Enumerable.Empty<PackageInProject>()));
            Assert.That(exception.ParamName, Is.EqualTo("currentPackages"));
        }

        [Test]
        public void OneUpdate_IsValid()
        {
            var fooVersionFour = new PackageIdentity("foo", VersionFour());
            var highest = new PackageSearchMedatadata(fooVersionFour, ASource, DateTimeOffset.Now);

            var match = LatestFooMetadata();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, highest, match, currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(LatestVersionOfPackageFoo()));
            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(match.Identity.Version));
            Assert.That(updates.Match.Source, Is.EqualTo(ASource));
            Assert.That(updates.HighestVersion, Is.EqualTo(VersionFour()));
            Assert.That(updates.AllowedChange, Is.EqualTo(VersionChange.Major));
        }

        [Test]
        public void OneUpdate_HasCorrectCurrentPackages()
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages);

            Assert.That(updates.CurrentPackages, Is.Not.Null);
            Assert.That(updates.CurrentPackages.Count, Is.EqualTo(1));
            Assert.That(updates.CurrentPackages.First().Id, Is.EqualTo("foo"));
        }

        [Test]
        public void TwoUpdates_AreValid()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(LatestVersionOfPackageFoo()));

            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(newPackage.Version));
        }

        [Test]
        public void TwoUpdates_HaveCorrectCurrentPackages()
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages);

            Assert.That(updates.CurrentPackages, Is.Not.Null);
            var currents = updates.CurrentPackages.ToList();

            Assert.That(currents.Count, Is.EqualTo(2));
            Assert.That(currents[0].Id, Is.EqualTo("foo"));
            Assert.That(currents[0].Version, Is.EqualTo(new NuGetVersion("1.0.0")));

            Assert.That(currents[1].Id, Is.EqualTo("foo"));
            Assert.That(currents[1].Version, Is.EqualTo(new NuGetVersion("1.0.1")));
        }

        [Test]
        public void CannotHaveUpdateForDifferentPackageToNewVersion()
        {
            var currentPackageBar = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackageBar));
        }

        [Test]
        public void WhenPackageDoesNotMatch_ExceptionMessageContainsMismatchedPackages()
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.0", PathToProjectOne()),
                new PackageInProject("bar", "1.0.0", PathToProjectTwo()),
                new PackageInProject("fish", "1.0.0", PathToProjectOne())
            };

            var ex = Assert.Throws<ArgumentException>(() => new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages));

            Assert.That(ex.Message, Is.EqualTo("Updates must all be for package 'foo', got 'bar, fish'"));
        }

        [Test]
        public void CannotHaveUpdateForDifferentPackagesInCurrentList()
        {
            var currentPackagesFooAndBar = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("bar", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackagesFooAndBar));
        }

        [Test]
        public void CountCurrentVersions_WhenThereIsOneUpdate()
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages);

            Assert.That(updates.CountCurrentVersions(), Is.EqualTo(1));
        }

        [Test]
        public void CountCurrentVersions_WhenThereAreTwoIdenticalUpdates()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages);

            Assert.That(updates.CountCurrentVersions(), Is.EqualTo(1));
        }

        [Test]
        public void CountCurrentVersions_WhenThereAreTwoDifferentUpdates()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var updates = new PackageUpdateSet(VersionChange.Major, LatestFooMetadata(), LatestFooMetadata(), currentPackages);

            Assert.That(updates.CountCurrentVersions(), Is.EqualTo(2));
        }

        private PackageIdentity LatestVersionOfPackageFoo()
        {
            return new PackageIdentity("foo", new NuGetVersion("1.2.3"));
        }

        private PackageSearchMedatadata LatestFooMetadata()
        {
            return new PackageSearchMedatadata(
                LatestVersionOfPackageFoo(),
                ASource, DateTimeOffset.Now);
        }

        private NuGetVersion VersionFour()
        {
            return new NuGetVersion("4.0.0");
        }

        private PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }

        private PackagePath PathToProjectTwo()
        {
            return new PackagePath("c_temp", "projectTwo", PackageReferenceType.PackagesConfig);
        }
    }
}
