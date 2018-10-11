using System;
using System.Collections.Generic;
using NuKeeper.Inspection.Report;
using NuKeeper.Inspection.Report.Formats;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Report.Formats
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
            AssertExpectedEmpty(reportType, outData);
        }

        [Test, TestCaseSource(nameof(AllReportFormats))]
        public void MultipleUpdatesInListCanBeWritten(Type reportType)
        {
            var rows = PackageUpdates.PackageUpdateSets(5);

            var outData = ReportToString(reportType, rows);

            Assert.That(outData, Is.Not.Null);

            AssertExpectedEmpty(reportType, outData);
        }

        private static void AssertExpectedEmpty(Type reportType, string outData)
        {
            var expectEmpty = reportType == typeof(NullReportFormat);
            if (expectEmpty)
            {
                Assert.That(outData, Is.Empty);
            }
            else
            {
                Assert.That(outData, Is.Not.Empty);
            }
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
            using (var output = new TestReportWriter())
            {
                var reporter = MakeInstance(reportType, output);
                reporter.Write("test", rows);
                return output.Data();
            }
        }

        private static IReportFormat MakeInstance(Type reportType, IReportWriter writer)
        {
            var noArgCtor = reportType.GetConstructor(Array.Empty<Type>());
            if (noArgCtor != null)
            {
                return (IReportFormat)noArgCtor.Invoke(Array.Empty<object>());
            }

            var oneArgCtor = reportType.GetConstructor(new [] { typeof(IReportWriter) } );
            return (IReportFormat)oneArgCtor.Invoke(new object[] { writer } );
        }
    }
}
