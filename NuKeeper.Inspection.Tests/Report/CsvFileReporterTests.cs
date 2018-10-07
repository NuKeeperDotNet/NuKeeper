using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Report
{
    [TestFixture]
    public class CsvFileReporterTests
    {
        [Test]
        public void NoRowsHasHeaderLineInOutput()
        {
            var rows = new List<PackageUpdateSet>();

            var output = ReportToString(rows);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Not.Empty);

            var lines = output.Split(Environment.NewLine);
            Assert.That(lines.Length, Is.EqualTo(1));
        }

        [Test]
        public void OneRowHasOutput()
        {
            var rows = OnePackageUpdateSet();

            var output = ReportToString(rows);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Not.Empty);

            var lines = output.Split(Environment.NewLine);
            Assert.That(lines.Length, Is.EqualTo(2));
        }

        [Test]
        public void OneRowHasMatchedCommas()
        {
            var rows = new List<PackageUpdateSet>();

            var output = ReportToString(rows);
            var lines = output.Split(Environment.NewLine);

            foreach (var line in lines)
            {
                var commas = line.Count(c => c == ',');
                Assert.That(commas, Is.EqualTo(11), $"Failed on line {line}");
            }
        }

        [Test]
        public void TwoRowsHaveOutput()
        {
            var package1 = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));
            var package2 = new PackageIdentity("fish", new NuGetVersion("2.3.4"));

            var rows = new List<PackageUpdateSet>
            {
                UpdateSetFor(package1, MakePackageForV110(package1)),
                UpdateSetFor(package2, MakePackageForV110(package2))
            };

            var output = ReportToString(rows);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Not.Empty);

            var lines = output.Split(Environment.NewLine);
            Assert.That(lines.Length, Is.EqualTo(3));
            Assert.That(lines[1], Does.Contain("foo.bar,"));
            Assert.That(lines[2], Does.Contain("fish,"));
        }

        private static string ReportToString(List<PackageUpdateSet> rows)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            var reporter = new CsvReportFormat(streamSource, Substitute.For<INuKeeperLogger>());

            reporter.Write("test", rows);

            return StreamToString(memoryStream);
        }

        private static string StreamToString(MemoryStream memoryStream)
        {
            var data = Encoding.UTF8.GetString(memoryStream.ToArray());
            return data.Trim();
        }

        private static PackageUpdateSet UpdateSetFor(PackageIdentity package, params PackageInProject[] packages)
        {
            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(package, new PackageSource("http://none"), publishedDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        private static PackageInProject MakePackageForV110(PackageIdentity package)
        {
            var path = new PackagePath(
                OsSpecifics.GenerateBaseDirectory(),
                Path.Combine("folder", "src", "project1", "packages.config"),
                PackageReferenceType.PackagesConfig);
            return new PackageInProject(package.Id, package.Version.ToString(), path);
        }
        private static List<PackageUpdateSet> OnePackageUpdateSet()
        {
            var package = new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));

            return new List<PackageUpdateSet>
            {
                UpdateSetFor(package, MakePackageForV110(package))
            };
        }
    }
}
