using System;
using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Engine;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class CommitReportTests
    {
        [Test]
        public void MakeCommitMessage_OneUpdateIsCorrect()
        {
            var updates = new List<PackageUpdate> { MakePackageUpdateFromV110() };

            var report = CommitReport.MakeCommitMessage(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void MakeCommitMessage_TwoUpdatesIsCorrect()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV100()
            };

            var report = CommitReport.MakeCommitMessage(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void MakeCommitMessage_TwoUpdatesSameVersionIsCorrect()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV110InProject3()
            };

            var report = CommitReport.MakeCommitMessage(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("Automatic update of foo.bar to 1.2.3"));
        }


        [Test]
        public void OneUpdate_MakeCommitDetails_IsNotEmpty()
        {
            var updates = new List<PackageUpdate> { MakePackageUpdateFromV110() };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasStandardTexts()
        {
            var updates = new List<PackageUpdate> { MakePackageUpdateFromV110() };

            var report = CommitReport.MakeCommitDetails(updates);

            AssertContainsStandardText(report);
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasVersionInfo()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated an update of `foo.bar` to `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasProjectDetails()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("1 project update:"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project1\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_NotEmpty()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV100()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_HasStandardTexts()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV100()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            AssertContainsStandardText(report);
            Assert.That(report, Does.Contain("1.0.0"));
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_HasVersionInfo()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV100()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated an update of `foo.bar` to `1.2.3`"));
            Assert.That(report, Does.Contain("2 versions of `foo.bar` were found in use: `1.1.0`,`1.0.0`"));
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_HasProjectList()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV100()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("2 project updates:"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project1\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project2\\packages.config` to `foo.bar` `1.2.3` from `1.0.0`"));
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_NotEmpty()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV110InProject3()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_HasStandardTexts()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV110InProject3()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            AssertContainsStandardText(report);
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_HasVersionInfo()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV110InProject3()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated an update of `foo.bar` to `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_HasProjectList()
        {
            var updates = new List<PackageUpdate>
            {
                MakePackageUpdateFromV110(),
                MakePackageUpdateFromV110InProject3()
            };

            var report = CommitReport.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("2 project updates:"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project1\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project3\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
        }


        private static void AssertContainsStandardText(string report)
        {
            Assert.That(report, Does.StartWith("NuKeeper has generated an update of `foo.bar` to `1.2.3`"));
            Assert.That(report, Does.Contain("This is an automated update. Merge only if it passes tests"));
            Assert.That(report, Does.EndWith("**NuKeeper**: https://github.com/NuKeeperDotNet/NuKeeper" + Environment.NewLine));
            Assert.That(report, Does.Contain("1.1.0"));

            Assert.That(report, Does.Not.Contain("Exception"));
            Assert.That(report, Does.Not.Contain("System.String"));
            Assert.That(report, Does.Not.Contain("Generic"));
            Assert.That(report, Does.Not.Contain("["));
            Assert.That(report, Does.Not.Contain("]"));
        }

        private static PackageUpdate MakePackageUpdateFromV110()
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project1\\packages.config", PackageReferenceType.PackagesConfig);

            var currentPackage = new PackageInProject("foo.bar", "1.1.0", path);
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            return new PackageUpdate(currentPackage, newPackage);
        }

        private static PackageUpdate MakePackageUpdateFromV110InProject3()
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project3\\packages.config", PackageReferenceType.PackagesConfig);

            var currentPackage = new PackageInProject("foo.bar", "1.1.0", path);
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            return new PackageUpdate(currentPackage, newPackage);
        }

        private static PackageUpdate MakePackageUpdateFromV100()
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project2\\packages.config", PackageReferenceType.PackagesConfig);

            var currentPackage = new PackageInProject("foo.bar", "1.0.0", path);
            var newPackage = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            return new PackageUpdate(currentPackage, newPackage);
        }
    }
}
