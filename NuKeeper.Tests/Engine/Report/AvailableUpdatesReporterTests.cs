using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Engine.Report;
using NuKeeper.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine.Report
{
    [TestFixture]
    public class AvailableUpdatesReporterTests
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
            var rows = new List<PackageUpdateSet>
            {
                UpdateSetFor(MakePackageForV110())
            };

            var output = ReportToString(rows);

            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Not.Empty);

            var lines = output.Split(Environment.NewLine);
            Assert.That(lines.Length, Is.EqualTo(2));
        }

        [Test]
        public void OneRowHasMatchedCommas()
        {
            var rows = new List<PackageUpdateSet>
            {
                UpdateSetFor(MakePackageForV110())
            };

            var output = ReportToString(rows);
            var lines = output.Split(Environment.NewLine);

            foreach (var line in lines)
            {
                var commas = line.Count(c => c == ',');
                Assert.That(commas, Is.EqualTo(11), $"Failed on line {line}");
            }
        }

        private static string ReportToString(List<PackageUpdateSet> rows)
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            var streamSource = Substitute.For<IReportStreamSource>();
            streamSource.GetStream(Arg.Any<string>())
                .Returns(writer);

            var reporter = new AvailableUpdatesReporter(streamSource, new NullNuKeeperLogger());

            reporter.Report("test", rows);

            return StreamToString(memoryStream);
        }

        private static string StreamToString(MemoryStream memoryStream)
        {
            var data = Encoding.UTF8.GetString(memoryStream.ToArray());
            return data.Trim();
        }

        private static PackageUpdateSet UpdateSetFor(params PackageInProject[] packages)
        {
            var newPackage = NewPackageFooBar123();

            var publishedDate = new DateTimeOffset(2018, 2, 19, 11, 12, 7, TimeSpan.Zero);
            var latest = new PackageSearchMedatadata(newPackage, "someSource", publishedDate);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null);
            return new PackageUpdateSet(updates, packages);
        }

        private static PackageInProject MakePackageForV110()
        {
            var path = new PackagePath("c:\\temp", "folder\\src\\project1\\packages.config",
                PackageReferenceType.PackagesConfig);
            return new PackageInProject("foo.bar", "1.1.0", path);
        }

        private static PackageIdentity NewPackageFooBar123()
        {
            return new PackageIdentity("foo.bar", new NuGetVersion("1.2.3"));
        }
    }
}
