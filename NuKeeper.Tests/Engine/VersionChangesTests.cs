using System;
using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class VersionChangesTests
    {
        [Test]
        public void WhenThereAreNoCandidates()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>();

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major, Is.Null);
            Assert.That(result.Minor, Is.Null);
            Assert.That(result.Patch, Is.Null);
            Assert.That(result.Selected(), Is.Null);
        }

        [Test]
        public void WhenThereIsANewMajorVersion()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(2, 3, 4)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.Major, Is.Not.Null);
            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));

            Assert.That(result.Minor, Is.Null);
            Assert.That(result.Patch, Is.Null);
        }

        [Test]
        public void WhenThereAreNewMajorMinorAndPatchVersion()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(2, 3, 4),
                MetadataForVersion(1, 3, 4),
                MetadataForVersion(1, 2, 4)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major, Is.Not.Null);
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));

            Assert.That(result.Minor, Is.Not.Null);
            Assert.That(result.Minor.Identity.Version, Is.EqualTo(new NuGetVersion(1, 3, 4)));

            Assert.That(result.Patch, Is.Not.Null);
            Assert.That(result.Patch.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 4)));

            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Selected().Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4)));
        }

        [Test]
        public void WhenMinorChangesAreAllowed()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(2, 3, 4),
                MetadataForVersion(1, 3, 4),
                MetadataForVersion(1, 2, 4)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Minor);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Minor));
            Assert.That(result.Major, Is.Not.Null);
            Assert.That(result.Minor, Is.Not.Null);
            Assert.That(result.Patch, Is.Not.Null);

            Assert.That(result.Selected(), Is.EqualTo(result.Minor));
            Assert.That(result.Selected().Identity.Version, Is.EqualTo(new NuGetVersion(1, 3, 4)));
        }

        [Test]
        public void WhenPatchChangesAreAllowed()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(2, 3, 4),
                MetadataForVersion(1, 3, 4),
                MetadataForVersion(1, 2, 4)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Patch);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Patch));
            Assert.That(result.Major, Is.Not.Null);
            Assert.That(result.Minor, Is.Not.Null);
            Assert.That(result.Patch, Is.Not.Null);

            Assert.That(result.Selected(), Is.EqualTo(result.Patch));
            Assert.That(result.Selected().Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 4)));
        }

        [Test]
        public void WhenThereAreMultipleVersionsOutOfOrder()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(1, 3, 4),
                MetadataForVersion(2, 3, 4),
                MetadataForVersion(1, 2, 4),
                MetadataForVersion(3, 3, 4),
                MetadataForVersion(1, 5, 6),
                MetadataForVersion(2, 5, 6),
                MetadataForVersion(5, 6, 7),
                MetadataForVersion(1, 1, 1),
                MetadataForVersion(1, 2, 9),
                MetadataForVersion(0, 1, 1)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Selected(), Is.EqualTo(result.Major));

            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(5, 6, 7)));
            Assert.That(result.Minor.Identity.Version, Is.EqualTo(new NuGetVersion(1, 5, 6)));
            Assert.That(result.Patch.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 9)));
        }

        private PackageSearchMedatadata MetadataForVersion(int major, int minor, int patch)
        {
            var version = new NuGetVersion(major, minor, patch);
            var packageId = new PackageIdentity("foo", version);
            return new PackageSearchMedatadata(packageId, "aSource", DateTimeOffset.Now);
        }
    }
}
