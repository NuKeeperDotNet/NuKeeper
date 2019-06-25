using System;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Inspection.NuGetApi;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.NuGetApi
{
    [TestFixture]
    public class PackageLookupResultReporterTests
    {
        [Test]
        public void CanCopeWithEmptyData()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var data = new PackageLookupResult(VersionChange.Major, null, null, null);

            reporter.Report(data);

            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Minimal(Arg.Any<string>());
            logger.DidNotReceive().Normal(Arg.Any<string>());
            logger.DidNotReceive().Detailed(Arg.Any<string>());
        }


        [Test]
        public void WhenThereIsAMajorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMetadata = new PackageSearchMetadata(
                new PackageIdentity("foo", new NuGetVersion(2, 3, 4)),
                new PackageSource("http://none"), DateTimeOffset.Now, null);

            var data = new PackageLookupResult(VersionChange.Major, fooMetadata, fooMetadata, fooMetadata);

            reporter.Report(data);

            logger.Received()
                .Detailed("Selected update of package foo to highest version, 2.3.4.");
            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Minimal(Arg.Any<string>());
            logger.DidNotReceive().Normal(Arg.Any<string>());
        }

        [Test]
        public void WhenThereIsAMinorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMetadata = new PackageSearchMetadata(
                new PackageIdentity("foo", new NuGetVersion(2, 3, 4)),
                new PackageSource("http://none"), DateTimeOffset.Now, null);

            var data = new PackageLookupResult(VersionChange.Minor, fooMetadata, fooMetadata, fooMetadata);

            reporter.Report(data);

            logger.Received()
                .Detailed("Selected update of package foo to highest version, 2.3.4. Allowing Minor version updates.");

            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Minimal(Arg.Any<string>());
            logger.DidNotReceive().Normal(Arg.Any<string>());
        }

        [Test]
        public void WhenThereIsAMajorAndAMinorUpdate()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMajor = new PackageSearchMetadata(
                new PackageIdentity("foo", new NuGetVersion(3, 0, 0)),
                new PackageSource("http://none"), DateTimeOffset.Now, null);
            var fooMinor = new PackageSearchMetadata(
                new PackageIdentity("foo", new NuGetVersion(2, 3, 4)),
                new PackageSource("http://none"), DateTimeOffset.Now, null);

            var data = new PackageLookupResult(VersionChange.Minor, fooMajor, fooMinor, null);

            reporter.Report(data);

            logger.Received()
                .Normal("Selected update of package foo to version 2.3.4, but version 3.0.0 is also available. Allowing Minor version updates.");
            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Minimal(Arg.Any<string>());
            logger.DidNotReceive().Detailed(Arg.Any<string>());
        }

        [Test]
        public void WhenThereIsAMajorUpdateThatCannotBeUsed()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var reporter = new PackageLookupResultReporter(logger);

            var fooMajor = new PackageSearchMetadata(
                new PackageIdentity("foo", new NuGetVersion(3, 0, 0)),
                new PackageSource("http://none"), DateTimeOffset.Now, null);

            var data = new PackageLookupResult(VersionChange.Minor, fooMajor, null, null);

            reporter.Report(data);

            logger.Received()
                .Normal("Package foo version 3.0.0 is available but is not allowed. Allowing Minor version updates.");
            logger.DidNotReceive().Error(Arg.Any<string>());
            logger.DidNotReceive().Minimal(Arg.Any<string>());
            logger.DidNotReceive().Detailed(Arg.Any<string>());
        }
    }
}
