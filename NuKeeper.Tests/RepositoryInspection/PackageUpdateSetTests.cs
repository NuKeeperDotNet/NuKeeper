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
        public void NullNewPackageId_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var exception = Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(
                null, ASource, VersionFour(), VersionChange.Major, packages));

            Assert.That(exception.ParamName, Is.EqualTo("newPackage"));
        }

        [Test]
        public void NullMaxVersion_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var exception = Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(
                LatestVersionOfPackageFoo(), ASource, null, VersionChange.Major, packages));

            Assert.That(exception.ParamName, Is.EqualTo("highest"));
        }

        [Test]
        public void NullPackages_IsNotAllowed()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(
                LatestVersionOfPackageFoo(), ASource, VersionFour(), VersionChange.Major, null));

            Assert.That(exception.ParamName, Is.EqualTo("currentPackages"));
        }

        [Test]
        public void NullSource_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var exception = Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(
                LatestVersionOfPackageFoo(), null, VersionFour(), VersionChange.Major, packages));
            Assert.That(exception.ParamName, Is.EqualTo("packageSource"));
        }

        [Test]
        public void EmptySource_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var exception = Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(
                LatestVersionOfPackageFoo(), string.Empty, VersionFour(), VersionChange.Major, packages));
            Assert.That(exception.ParamName, Is.EqualTo("packageSource"));
        }

        [Test]
        public void EmptyPackages_IsNotAllowed()
        {
            var exception = Assert.Throws<ArgumentException>(() => new PackageUpdateSet(
                LatestVersionOfPackageFoo(), ASource, VersionFour(), VersionChange.Major,
                Enumerable.Empty<PackageInProject>()));
            Assert.That(exception.ParamName, Is.EqualTo("currentPackages"));
        }

        [Test]
        public void OneUpdate_IsValid()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(LatestVersionOfPackageFoo()));
            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(newPackage.Version));
            Assert.That(updates.PackageSource, Is.EqualTo(ASource));
            Assert.That(updates.Highest, Is.EqualTo(VersionFour()));
            Assert.That(updates.AllowedChange, Is.EqualTo(VersionChange.Major));
        }

        [Test]
        public void OneUpdate_HasCorrectCurrentPackages()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

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

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(LatestVersionOfPackageFoo()));

            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(newPackage.Version));
        }

        [Test]
        public void TwoUpdates_HaveCorrectCurrentPackages()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

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
            var newPackageFoo = LatestVersionOfPackageFoo();

            var currentPackageBar = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(
                newPackageFoo, ASource, VersionFour(), VersionChange.Major, currentPackageBar));
        }

        [Test]
        public void WhenPackageDoesNotMatch_ExceptionMessageContainsMismatchedPackages()
        {
            var newPackageFoo = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.0", PathToProjectOne()),
                new PackageInProject("bar", "1.0.0", PathToProjectTwo()),
                new PackageInProject("fish", "1.0.0", PathToProjectOne())
            };

            var ex = Assert.Throws<ArgumentException>(() => new PackageUpdateSet(
                newPackageFoo, ASource, VersionFour(), VersionChange.Major, currentPackages));

            Assert.That(ex.Message, Is.EqualTo("Updates must all be for package 'foo', got 'bar, fish'"));
        }

        [Test]
        public void CannotHaveUpdateForDifferentPackagesInCurrentList()
        {
            var newPackageFoo = LatestVersionOfPackageFoo();

            var currentPackagesFooAndBar = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("bar", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(
                newPackageFoo, ASource, VersionFour(), VersionChange.Major, currentPackagesFooAndBar));
        }

        [Test]
        public void CountCurrentVersions_WhenThereIsOneUpdate()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

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

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

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

            var updates = new PackageUpdateSet(newPackage, ASource, VersionFour(), VersionChange.Major, currentPackages);

            Assert.That(updates.CountCurrentVersions(), Is.EqualTo(2));
        }

        private PackageIdentity LatestVersionOfPackageFoo()
        {
            return new PackageIdentity("foo", new NuGetVersion("1.2.3"));
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
