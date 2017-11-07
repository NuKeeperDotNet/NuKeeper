using NSubstitute;
using NuGet.Packaging.Core;
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

            var data = new PackageLookupResult(VersionChange.Major, null, null);

            reporter.Report(data);

            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Terse(Arg.Any<string>());
            logger.DidNotReceive().Info(Arg.Any<string>());
            logger.DidNotReceive().Verbose(Arg.Any<string>());
        }


        [Test]
        public void WhenThereIsAMajorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMetadata = new PackageSearchMedatadata(
                new PackageIdentity("foo", new NuGetVersion(2, 3, 4)), 
                "someSource");

            var data = new PackageLookupResult(VersionChange.Major, fooMetadata, fooMetadata);

            reporter.Report(data);

            logger.Received()
                .Verbose("Selected update of package foo to highest version, 2.3.4.");
            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Terse(Arg.Any<string>());
            logger.DidNotReceive().Info(Arg.Any<string>());
        }

        [Test]
        public void WhenThereIsAMinorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMetadata = new PackageSearchMedatadata(
                new PackageIdentity("foo", new NuGetVersion(2, 3, 4)),
                "someSource");

            var data = new PackageLookupResult(VersionChange.Minor, fooMetadata, fooMetadata);

            reporter.Report(data);

            logger.Received()
                .Verbose("Selected update of package foo to highest version, 2.3.4. Allowing Minor version updates.");

            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Terse(Arg.Any<string>());
            logger.DidNotReceive().Info(Arg.Any<string>());
        }

        [Test]
        public void WhenThereIsAMajorAndAMinorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMajor = new PackageSearchMedatadata(
                new PackageIdentity("foo", new NuGetVersion(3, 0, 0)),
                "someSource");
            var fooMinor = new PackageSearchMedatadata(
                new PackageIdentity("foo", new NuGetVersion(2, 3, 4)),
                "someSource");

            var data = new PackageLookupResult(VersionChange.Minor, fooMajor, fooMinor);

            reporter.Report(data);

            logger.Received()
                .Info("Selected update of package foo to version 2.3.4, but version 3.0.0 is also available. Allowing Minor version updates.");
            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Terse(Arg.Any<string>());
            logger.DidNotReceive().Verbose(Arg.Any<string>());
        }

        [Test]
        public void WhenThereIsAMajorUpdateThatCannotBeUsed()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMajor = new PackageSearchMedatadata(
                new PackageIdentity("foo", new NuGetVersion(3, 0, 0)),
                "someSource");

            var data = new PackageLookupResult(VersionChange.Minor, fooMajor, null);

            reporter.Report(data);

            logger.Received()
                .Info("Package foo version 3.0.0 is available but is not allowed. Allowing Minor version updates.");
            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Terse(Arg.Any<string>());
            logger.DidNotReceive().Verbose(Arg.Any<string>());
        }
    }
}
