using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Nuget.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Nuget.Api
{
    [TestFixture]
    public class PackageUpdateSetTests
    {
        [Test]
        public void NullNewPackageId_notAllowed()
        {
            var packages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(null, packages));
        }

        [Test]
        public void NullPackages_notAllowed()
        {
            Assert.Throws<ArgumentNullException>(() => new PackageUpdateSet(NewPackageId(), null));
        }
        [Test]
        public void EmptyPackages_notAllowed()
        {
            Assert.Throws<ArgumentException>(
                () => new PackageUpdateSet(NewPackageId(), Enumerable.Empty<PackageInProject>()));
        }

        [Test]
        public void OneUpdate_IsValid()
        {
            var newPackage = NewPackageId();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne())
            };

            var updates = new PackageUpdateSet(newPackage, currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(NewPackageId()));
            Assert.That(updates.CurrentPackages, Is.Not.Null);
            Assert.That(updates.CurrentPackages.Count, Is.EqualTo(1));

            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(newPackage.Version));
        }

        [Test]
        public void TwoUpdates_AreValid()
        {
            var newPackage = NewPackageId();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo())
            };

            var updates = new PackageUpdateSet(newPackage, currentPackages);

            Assert.That(updates, Is.Not.Null);
            Assert.That(updates.NewPackage, Is.EqualTo(NewPackageId()));
            Assert.That(updates.CurrentPackages, Is.Not.Null);
            Assert.That(updates.CurrentPackages.Count, Is.EqualTo(2));

            Assert.That(updates.PackageId, Is.EqualTo("foo"));
            Assert.That(updates.NewVersion, Is.EqualTo(newPackage.Version));
        }

        [Test]
        public void CannotHaveUpdateForDifferentPackageToNewVersion()
        {
            var newPackage = NewPackageId();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("bar", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(newPackage, currentPackages));
        }

        [Test]
        public void CannotHaveUpdateForDifferentPackagesInCurrentList()
        {
            var newPackage = NewPackageId();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.0", PathToProjectOne()),
                new PackageInProject("bar", "1.0.0", PathToProjectOne())
            };

            Assert.Throws<ArgumentException>(() => new PackageUpdateSet(newPackage, currentPackages));
        }

        private PackageIdentity NewPackageId()
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
