using System;
using System.Collections.Generic;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Report
{
    [TestFixture]
    public class ReportFormatTests
    {
        [Test, TestCaseSource("AllReportFormats")]
        public void EmptyUpdateListCanBeWritten(Type reportType)
        {
            var rows = new List<PackageUpdateSet>();

            var outData = ReportToString(reportType, rows);

            Assert.That(outData, Is.Not.Null);
        }

        [Test, TestCaseSource("AllReportFormats")]
        public void OneUpdateInListCanBeWritten(Type reportType)
        {
            var rows = PackageUpdates.OnePackageUpdateSet();

            var outData = ReportToString(reportType, rows);

            Assert.That(outData, Is.Not.Null);
        }

        [Test, TestCaseSource("AllReportFormats")]
        public void MultipleUpdatesInListCanBeWritten(Type reportType)
        {
            var rows = PackageUpdates.PackageUpdateSets(5);

            var outData = ReportToString(reportType, rows);

            Assert.That(outData, Is.Not.Null);
        }

        public static Type[] AllReportFormats()
        {
            return new[]
            {
                typeof(TextReportFormat),
                typeof(CsvReportFormat)
            };
        }

        private static string ReportToString(Type reportType, List<PackageUpdateSet> rows)
        {
            var output = new TestReportWriter();

            var reporter = MakeInstance(reportType, output);
            reporter.Write("test", rows);

            return output.Data();
        }

        private static IReportFormat MakeInstance(Type reportType, TestReportWriter output)
        {
            var ctor = reportType.GetConstructor(new Type[] { typeof(IReportWriter) } );
            return (IReportFormat)ctor.Invoke(new object[] { output } );
        }
    }
}
