using System;
using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.NuGetApi;
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
        public void WhenThereAreNewMajorMinorPatchAndReleaseVersion()
        {
            var current = new NuGetVersion(1, 2, 3, "preview004");
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(2, 3, 4, "preview005"),
                MetadataForVersion(1, 3, 4, "preview005"),
                MetadataForVersion(1, 2, 4, "preview005")
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major, Is.Not.Null);
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4, "preview005")));

            Assert.That(result.Minor, Is.Not.Null);
            Assert.That(result.Minor.Identity.Version, Is.EqualTo(new NuGetVersion(1, 3, 4, "preview005")));

            Assert.That(result.Patch, Is.Not.Null);
            Assert.That(result.Patch.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 4, "preview005")));

            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Selected().Identity.Version, Is.EqualTo(new NuGetVersion(2, 3, 4, "preview005")));
        }


        [Test]
        public void WhenThereAreOnlyNewPatchVersion()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(1, 2, 7),
                MetadataForVersion(1, 2, 5),
                MetadataForVersion(1, 2, 4),
                MetadataForVersion(1, 2, 6),
                MetadataForVersion(1, 2, 8)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 8)));

            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Selected(), Is.EqualTo(result.Minor));
            Assert.That(result.Selected(), Is.EqualTo(result.Patch));
            Assert.That(result.Major, Is.EqualTo(result.Minor));
            Assert.That(result.Major, Is.EqualTo(result.Patch));
        }

        [Test]
        public void WhenThereAreOnlyNewReleaseVersion()
        {
            var current = new NuGetVersion(1, 2, 3, "preview004");
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(1, 2, 3, "preview007"),
                MetadataForVersion(1, 2, 3, "preview005"),
                MetadataForVersion(1, 2, 3, "preview004"),
                MetadataForVersion(1, 2, 3, "preview006"),
                MetadataForVersion(1, 2, 3, "preview008")
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 3, "preview008")));

            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Selected(), Is.EqualTo(result.Minor));
            Assert.That(result.Selected(), Is.EqualTo(result.Patch));
            Assert.That(result.Selected(), Is.EqualTo(result.Release));
            Assert.That(result.Major, Is.EqualTo(result.Minor));
            Assert.That(result.Major, Is.EqualTo(result.Patch));
            Assert.That(result.Major, Is.EqualTo(result.Release));
        }

        [Test]
        public void WhenThereAreOnlyNewMinorAndPatchVersion()
        {
            var current = new NuGetVersion(1, 2, 3);
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(1, 2, 7),
                MetadataForVersion(1, 3, 5),
                MetadataForVersion(1, 2, 4),
                MetadataForVersion(1, 4, 1),
                MetadataForVersion(1, 2, 6),
                MetadataForVersion(1, 2, 8),
                MetadataForVersion(1, 3, 7)
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(1, 4, 1)));
            Assert.That(result.Minor.Identity.Version, Is.EqualTo(new NuGetVersion(1, 4, 1)));
            Assert.That(result.Patch.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 8)));

            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Selected(), Is.EqualTo(result.Minor));
            Assert.That(result.Selected(), Is.Not.EqualTo(result.Patch));

            Assert.That(result.Major, Is.EqualTo(result.Minor));
            Assert.That(result.Major, Is.Not.EqualTo(result.Patch));
        }

        [Test]
        public void WhenThereAreOnlyNewPatchAndReleaseVersion()
        {
            var current = new NuGetVersion(1, 2, 2, "preview004");
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(1, 2, 2, "preview007"),
                MetadataForVersion(1, 2, 3, "preview005"),
                MetadataForVersion(1, 2, 2, "preview004"),
                MetadataForVersion(1, 2, 4, "preview001"),
                MetadataForVersion(1, 2, 2, "preview006"),
                MetadataForVersion(1, 2, 2, "preview008"),
                MetadataForVersion(1, 2, 3, "preview007")
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 4, "preview001")));
            Assert.That(result.Minor.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 4, "preview001")));
            Assert.That(result.Patch.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 4, "preview001")));
            Assert.That(result.Release.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 2, "preview008")));

            Assert.That(result.Selected(), Is.EqualTo(result.Major));
            Assert.That(result.Selected(), Is.EqualTo(result.Minor));
            Assert.That(result.Selected(), Is.EqualTo(result.Patch));
            Assert.That(result.Selected(), Is.Not.EqualTo(result.Release));

            Assert.That(result.Major, Is.EqualTo(result.Minor));
            Assert.That(result.Major, Is.EqualTo(result.Patch));
            Assert.That(result.Major, Is.Not.EqualTo(result.Release));
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
        public void WhenReleaseChangesAreAllowed()
        {
            var current = new NuGetVersion(1, 2, 3, "preview004");
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(2, 3, 4, "preview005"),
                MetadataForVersion(1, 3, 4, "preview005"),
                MetadataForVersion(1, 4, 5, "preview005"),
                MetadataForVersion(1, 2, 3, "preview005")
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Release);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Release));
            Assert.That(result.Major, Is.Not.Null);
            Assert.That(result.Minor, Is.Not.Null);
            Assert.That(result.Patch, Is.Not.Null);
            Assert.That(result.Release, Is.Not.Null);

            Assert.That(result.Selected(), Is.EqualTo(result.Release));
            Assert.That(result.Selected().Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 3, "preview005")));
        }

        [Test]
        public void WhenThereAreMultipleVersionsOutOfOrder()
        {
            var current = new NuGetVersion(1, 2, 3, "preview004");
            var candidates = new List<PackageSearchMedatadata>
            {
                MetadataForVersion(1, 3, 4, "preview005"),
                MetadataForVersion(2, 3, 4, "preview006"),
                MetadataForVersion(1, 2, 4),
                MetadataForVersion(3, 3, 4, "preview005"),
                MetadataForVersion(1, 5, 6),
                MetadataForVersion(2, 5, 6, "preview002"),
                MetadataForVersion(5, 6, 7, "preview009"),
                MetadataForVersion(1, 1, 1),
                MetadataForVersion(1, 4, 6, "preview002"),
                MetadataForVersion(1, 3, 9, "preview007"),
                MetadataForVersion(1, 2, 8),
                MetadataForVersion(0, 1, 1, "preview005")
            };

            var result = VersionChanges.MakeVersions(current, candidates, VersionChange.Major);

            Assert.That(result.AllowedChange, Is.EqualTo(VersionChange.Major));
            Assert.That(result.Selected(), Is.EqualTo(result.Major));

            Assert.That(result.Major.Identity.Version, Is.EqualTo(new NuGetVersion(5, 6, 7, "preview009")));
            Assert.That(result.Minor.Identity.Version, Is.EqualTo(new NuGetVersion(1, 5, 6)));
            Assert.That(result.Patch.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 8)));
            Assert.That(result.Release.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 8)));
        }

        private PackageSearchMedatadata MetadataForVersion(int major, int minor, int patch, string release = "")
        {
            var version = new NuGetVersion(major, minor, patch, release);
            var packageId = new PackageIdentity("foo", version);
            return new PackageSearchMedatadata(packageId, new PackageSource("http://none"), DateTimeOffset.Now, null);
        }
    }
}
