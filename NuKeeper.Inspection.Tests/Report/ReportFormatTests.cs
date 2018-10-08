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
        [Test, TestCaseSource(nameof(AllReportFormats))]
        public void EmptyUpdateListCanBeWritten(Type reportType)
        {
            var rows = new List<PackageUpdateSet>();

            var outData = ReportToString(reportType, rows);

            Assert.That(outData, Is.Not.Null);
        }

        [Test, TestCaseSource(nameof(AllReportFormats))]
        public void OneUpdateInListCanBeWritten(Type reportType)
        {
            var rows = PackageUpdates.OnePackageUpdateSet();

            var outData = ReportToString(reportType, rows);

            Assert.That(outData, Is.Not.Null);
        }

        [Test, TestCaseSource(nameof(AllReportFormats))]
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
                typeof(NullReportFormat),
                typeof(TextReportFormat),
                typeof(CsvReportFormat),
                typeof(MetricsReportFormat),
                typeof(LibYearsReportFormat)
            };
        }

        private static string ReportToString(Type reportType, List<PackageUpdateSet> rows)
        {
            var output = new TestReportWriter();

            var reporter = MakeInstance(reportType, output);
            reporter.Write("test", rows);

            Assert.That(output.CloseCallCount, Is.EqualTo(1));

            return output.Data();
        }

        private static IReportFormat MakeInstance(Type reportType, IReportWriter writer)
        {
            var ctor = reportType.GetConstructor(new [] { typeof(IReportWriter) } );
            return (IReportFormat)ctor.Invoke(new object[] { writer } );
        }
    }
}
