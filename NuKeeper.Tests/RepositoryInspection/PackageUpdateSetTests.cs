using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class PackageUpdateSetTests
    {
        [Test]
        public void NullNewPackageId_IsNotAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(null, packages));
        }

        [Test]
        public void NullPackages_IsNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(LatestVersionOfPackageFoo(), null));
        }
        [Test]
        public void EmptyPackages_IsNotAllowed()
        {
            Assert.Throws<ArgumentException>(
                () => new PackageUpdateSet(LatestVersionOfPackageFoo(), Enumerable.Empty<PackageInProject>()));
        }

        [Test]
        public void OneUpdate_IsValid()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(newPackage, currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(LatestVersionOfPackageFoo()));
            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(newPackage.Version));
        }

        [Test]
        public void OneUpdate_HasCorrectCurrentPackages()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(newPackage, currentPackages);

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

            var updates = new PackageUpdateSet(newPackage, currentPackages);

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

            var updates = new PackageUpdateSet(newPackage, currentPackages);

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

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(newPackageFoo, currentPackageBar));
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

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(newPackageFoo, currentPackagesFooAndBar));
        }

        private PackageIdentity LatestVersionOfPackageFoo()
        {
            return new PackageIdentity("foo", new NuGetVersion("1.2.3"));
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
