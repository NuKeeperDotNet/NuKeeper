using System.Collections.Generic;
using System.IO;
using System.Text;
using NSubstitute;
using NuKeeper.Engine.Report;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine.Report
{
    [TestFixture]
    public class AvailableUpdatesReporterTests
    {
        [Test]
        public void NoRowsHasHeader()
        {
            var memoryStream = new MemoryStream();
            var writer = new StreamWriter(memoryStream);

            var streamSource = Substitute.For<IReportStreamSource>();
            streamSource.GetStream(Arg.Any<string>())
                .Returns(writer);

            var reporter = new AvailableUpdatesReporter(streamSource, new NullNuKeeperLogger());

            var rows = new List<PackageUpdateSet>();
            reporter.Report("test", rows);

            var output = Encoding.UTF8.GetString(memoryStream.ToArray());
            Assert.That(output, Is.Not.Null);
            Assert.That(output, Is.Not.Empty);
        }
    }
}
