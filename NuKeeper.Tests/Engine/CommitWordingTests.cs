using System;
using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Engine;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class CommitWordingTests
    {
        [Test]
        public void MarkPullRequestTitle_UpdateIsCorrect()
        {
            var updates = PackageUpdates.For(MakePackageForV110())
                .InList();

            var report = CommitWording.MakePullRequestTitle(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo("Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void MakeCommitMessage_OneUpdateIsCorrect()
        {
            var updates = PackageUpdates.For(MakePackageForV110());

            var report = CommitWording.MakeCommitMessage(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo(":package: Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void MakeCommitMessage_TwoUpdatesIsCorrect()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100());

            var report = CommitWording.MakeCommitMessage(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo(":package: Automatic update of foo.bar to 1.2.3"));
        }

        [Test]
        public void MakeCommitMessage_TwoUpdatesSameVersionIsCorrect()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV110InProject3());

            var report = CommitWording.MakeCommitMessage(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
            Assert.That(report, Is.EqualTo(":package: Automatic update of foo.bar to 1.2.3"));
        }


        [Test]
        public void OneUpdate_MakeCommitDetails_IsNotEmpty()
        {
            var updates = PackageUpdates.For(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasStandardTexts()
        {
            var updates = PackageUpdates.For(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            AssertContainsStandardText(report);
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasVersionInfo()
        {
            var updates = PackageUpdates.For(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated a minor update of `foo.bar` to `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasPublishedDate()
        {
            var updates = PackageUpdates.For(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("`foo.bar 1.2.3` was published at `2018-02-19T11:12:07Z`"));
        }


        [Test]
        public void OneUpdate_MakeCommitDetails_HasProjectDetails()
        {
            var updates = PackageUpdates.For(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("1 project update:"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project1\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_NotEmpty()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_HasStandardTexts()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            AssertContainsStandardText(report);
            Assert.That(report, Does.Contain("1.0.0"));
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_HasVersionInfo()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated a minor update of `foo.bar` to `1.2.3`"));
            Assert.That(report, Does.Contain("2 versions of `foo.bar` were found in use: `1.1.0`, `1.0.0`"));
        }

        [Test]
        public void TwoUpdates_MakeCommitDetails_HasProjectList()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV100())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("2 project updates:"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project1\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project2\\packages.config` to `foo.bar` `1.2.3` from `1.0.0`"));
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_NotEmpty()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV110InProject3())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Is.Not.Null);
            Assert.That(report, Is.Not.Empty);
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_HasStandardTexts()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV110InProject3())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            AssertContainsStandardText(report);
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_HasVersionInfo()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV110InProject3())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated a minor update of `foo.bar` to `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void TwoUpdatesSameVersion_MakeCommitDetails_HasProjectList()
        {
            var updates = PackageUpdates.For(MakePackageForV110(), MakePackageForV110InProject3())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("2 project updates:"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project1\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
            Assert.That(report, Does.Contain("Updated `folder\\src\\project3\\packages.config` to `foo.bar` `1.2.3` from `1.1.0`"));
        }

        [Test]
        public void OneUpdate_MakeCommitDetails_HasVersionLimitData()
        {
            var updates = PackageUpdates.LimitedToMinor(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("There is also a higher version, `foo.bar 2.3.4`, but this was not applied as only `Minor` version changes are allowed."));
        }

        [Test]
        public void OneUpdateWithDate_MakeCommitDetails_HasVersionLimitDataWithDate()
        {
            var publishedAt = new DateTimeOffset(2018, 2, 20, 11, 32 ,45, TimeSpan.Zero);

            var updates = PackageUpdates.LimitedToMinor(publishedAt, MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Contain("There is also a higher version, `foo.bar 2.3.4` published at `2018-02-20T11:32:45Z`,"));
            Assert.That(report, Does.Contain(" ago, but this was not applied as only `Minor` version changes are allowed."));
        }

        [Test]
        public void OneUpdateWithMajorVersionChange()
        {
            var updates = PackageUpdates.ForNewVersion(new PackageIdentity("foo.bar", new NuGetVersion("2.1.1")), MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated a major update of `foo.bar` to `2.1.1` from `1.1.0"));
        }

        [Test]
        public void OneUpdateWithMinorVersionChange()
        {
            var updates = PackageUpdates.ForNewVersion(new PackageIdentity("foo.bar", new NuGetVersion("1.2.1")), MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated a minor update of `foo.bar` to `1.2.1` from `1.1.0"));
        }

        [Test]
        public void OneUpdateWithPatchVersionChange()
        {
            var updates = PackageUpdates.ForNewVersion(new PackageIdentity("foo.bar", new NuGetVersion("1.1.9")), MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("NuKeeper has generated a patch update of `foo.bar` to `1.1.9` from `1.1.0"));
        }

        [Test]
        public void OneUpdateWithInternalPackageSource()
        {
            var updates = PackageUpdates.ForInternalSource(MakePackageForV110())
                .InList();

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.Not.Contain("on NuGet.org"));
            Assert.That(report, Does.Not.Contain("www.nuget.org"));
        }

        [Test]
        public void TwoUpdateSets()
        {
            var packageTwo = new PackageIdentity("packageTwo", new NuGetVersion("3.4.5"));

            var updates = new List<PackageUpdateSet>
            {
                PackageUpdates.ForNewVersion(new PackageIdentity("foo.bar", new NuGetVersion("2.1.1")), MakePackageForV110()),
                PackageUpdates.ForNewVersion(packageTwo, MakePackageForV110("packageTwo"))
            };

            var report = CommitWording.MakeCommitDetails(updates);

            Assert.That(report, Does.StartWith("2 packages were updated in 1 project:"));
            Assert.That(report, Does.Contain("`foo.bar`, `packageTwo`"));
            Assert.That(report, Does.Contain("<details>"));
            Assert.That(report, Does.Contain("</details>"));
            Assert.That(report, Does.Contain("<summary>"));
            Assert.That(report, Does.Contain("</summary>"));
            Assert.That(report, Does.Contain("NuKeeper has generated a major update of `foo.bar` to `2.1.1` from `1.1.0`"));
            Assert.That(report, Does.Contain("NuKeeper has generated a major update of `packageTwo` to `3.4.5` from `1.1.0`"));
        }

        private static void AssertContainsStandardText(string report)
        {
            Assert.That(report, Does.StartWith("NuKeeper has generated a minor update of `foo.bar` to `1.2.3`"));
            Assert.That(report, Does.Contain("This is an automated update. Merge only if it passes tests"));
            Assert.That(report, Does.EndWith("**NuKeeper**: https://github.com/NuKeeperDotNet/NuKeeper" + Environment.NewLine));
            Assert.That(report, Does.Contain("1.1.0"));
            Assert.That(report, Does.Contain("[foo.bar 1.2.3 on NuGet.org](https://www.nuget.org/packages/foo.bar/1.2.3)"));

            Assert.That(report, Does.Not.Contain("Exception"));
            Assert.That(report, Does.Not.Contain("System.String"));
            Assert.That(report, Does.Not.Contain("Generic"));
            Assert.That(report, Does.Not.Contain("[ "));
            Assert.That(report, Does.Not.Contain(" ]"));
            Assert.That(report, Does.Not.Contain("There is also a higher version"));
        }

        private static PackageInProject MakePackageForV110(string packageName = "foo.bar")
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project1\\packages.config",
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(packageName, "1.1.0", path);
        }

        private static PackageInProject MakePackageForV100()
        {
            var path2 = new PackagePath("c:\\temp", "folder\\src\\project2\\packages.config",
                PackageReferenceType.PackagesConfig);
            var currentPackage2 = new PackageInProject("foo.bar", "1.0.0", path2);
            return currentPackage2;
        }

        private static PackageInProject MakePackageForV110InProject3()
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project3\\packages.config", PackageReferenceType.PackagesConfig);

            return  new PackageInProject("foo.bar", "1.1.0", path);
        }
    }
}
