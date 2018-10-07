using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Report
{
    [TestFixture]
    public class CsvReportFormatTests
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
            var rows = PackageUpdates.OnePackageUpdateSet();

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
                PackageUpdates.UpdateSetFor(package1, PackageUpdates.MakePackageForV110(package1)),
                PackageUpdates.UpdateSetFor(package2, PackageUpdates.MakePackageForV110(package2))
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
            var output = new TestReportWriter();

            var reporter = new CsvReportFormat(output);
            reporter.Write("test", rows);

            return output.Data();
        }
    }
}
