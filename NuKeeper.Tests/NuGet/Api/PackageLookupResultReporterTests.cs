using NSubstitute;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuKeeper.Logging;
using NuKeeper.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Tests.NuGet.Api
{
    [TestFixture]
    public class PackageLookupResultReporterTests
    {
        [Test]
        public void CanCopeWithEmptyData()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var data = new PackageLookupResult();

            reporter.Report(data);

            logger.DidNotReceive().Info(Arg.Any<string>());
        }


        [Test]
        public void WhenThereIsAMajorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMetadata = new PackageSearchMedatadataWithSource("foo", MetadataWithVersion("foo", new NuGetVersion(2, 3, 4)));

            var data = new PackageLookupResult
            {
                AllowedChange = VersionChange.Major,
                Match = fooMetadata,
                Highest = fooMetadata
            };

            reporter.Report(data);

            logger.Received()
                .Info("Selected update to highest version, 2.3.4.");
        }

        [Test]
        public void WhenThereIsAMinorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMetadata = new PackageSearchMedatadataWithSource("foo", MetadataWithVersion("foo", new NuGetVersion(2, 3, 4)));

            var data = new PackageLookupResult
            {
                AllowedChange = VersionChange.Minor,
                Match = fooMetadata,
                Highest = fooMetadata
            };

            reporter.Report(data);

            logger.Received()
                .Info("Selected update to highest version, 2.3.4. Allowing Minor version updates.");
        }

        [Test]
        public void WhenThereIsAMajorAndAMinorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMajor = new PackageSearchMedatadataWithSource("foo", MetadataWithVersion("foo", new NuGetVersion(3, 0, 0)));
            var fooMinor = new PackageSearchMedatadataWithSource("foo", MetadataWithVersion("foo", new NuGetVersion(2, 3, 4)));

            var data = new PackageLookupResult
            {
                AllowedChange = VersionChange.Minor,
                Match = fooMinor,
                Highest = fooMajor
            };

            reporter.Report(data);

            logger.Received()
                .Info("Selected update to version 2.3.4, but version 3.0.0 is also available. Allowing Minor version updates.");
        }

        [Test]
        public void WhenThereIsAMajorUpdateThatCannotBeUsed()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMajor = new PackageSearchMedatadataWithSource("foo", MetadataWithVersion("foo", new NuGetVersion(3, 0, 0)));

            var data = new PackageLookupResult
            {
                AllowedChange = VersionChange.Minor,
                Match = null,
                Highest = fooMajor
            };

            reporter.Report(data);

            logger.Received()
                .Info("Version 3.0.0 is available but is not allowed. Allowing Minor version updates.");
        }


        private static IPackageSearchMetadata MetadataWithVersion(string id, NuGetVersion version)
        {
            var metadata = Substitute.For<IPackageSearchMetadata>();
            var identity = new PackageIdentity(id, version);
            metadata.Identity.Returns(identity);
            return metadata;
        }
    }
}
