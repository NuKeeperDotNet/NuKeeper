using System;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Versioning;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.RepositoryInspection
{
    [TestFixture]
    public class DirectoryBuildTargetsReaderTests
    {
        const string DirectoryBuildTargetsWithSinglePackageReference =
            @"<Project><ItemGroup><PackageReference Include=""foo"" Version=""1.2.3.4"" /></ItemGroup></Project>";

        private const string DirectoryBuildTargetsWithTwoPackageReferences = @"<Project><ItemGroup>
<PackageReference Include=""foo"" Version=""1.2.3.4"" />
<PackageReference Update=""bar"" Version=""2.3.4.5"" /></ItemGroup></Project>";

        const string DirectoryBuildTargetsWithSinglePackageDownload =
            @"<Project><ItemGroup><PackageDownload Include=""foo"" Version=""[1.2.3.4]"" /></ItemGroup></Project>";

        private const string DirectoryBuildTargetsWithTwoPackageDownloads = @"<Project><ItemGroup>
<PackageDownload Include=""foo"" Version=""[1.2.3.4]"" />
<PackageDownload Update=""bar"" Version=""[2.3.4.5]"" /></ItemGroup></Project>";

        [Test]
        public void EmptyPackagesListShouldBeParsed()
        {
            const string emptyContents =
                @"<Project/>";

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(emptyContents), TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void SinglePackageReferenceShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithSinglePackageReference), TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageReferenceShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithSinglePackageReference), TempPath());

            var package = packages.FirstOrDefault();
            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SinglePackageReferenceShouldBeCorrect()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithSinglePackageReference), TempPath());

            var package = packages.FirstOrDefault();

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void TwoPackageReferencesShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithTwoPackageReferences), TempPath())
                .ToList();

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages.Count, Is.EqualTo(2));

            PackageAssert.IsPopulated(packages[0]);
            PackageAssert.IsPopulated(packages[1]);
        }

        [Test]
        public void TwoPackageReferencesShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithTwoPackageReferences), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(2));

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4.5")));
        }

        [Test]
        public void PackageReferenceResultShouldBeReiterable()
        {
            var path = TempPath();

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithTwoPackageReferences), path);

            foreach (var package in packages)
            {
                PackageAssert.IsPopulated(package);
            }

            Assert.That(packages.Select(p => p.Path), Is.All.EqualTo(path));
        }

        [Test]
        public void WhenOnePackageCannotBeRead_TheOthersAreStillRead_WithPackageReference()
        {
            var badVersion = DirectoryBuildTargetsWithTwoPackageReferences.Replace("1.2.3.4", "notaversion", StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(badVersion), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            PackageAssert.IsPopulated(packages[0]);
        }

        [Test]
        public void SinglePackageDownloadShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithSinglePackageDownload), TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageDownloadShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithSinglePackageDownload), TempPath());

            var package = packages.FirstOrDefault();
            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SinglePackageDownloadShouldBeCorrect()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithSinglePackageDownload), TempPath());

            var package = packages.FirstOrDefault();

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void TwoPackageDownloadsShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithTwoPackageDownloads), TempPath())
                .ToList();

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages.Count, Is.EqualTo(2));

            PackageAssert.IsPopulated(packages[0]);
            PackageAssert.IsPopulated(packages[1]);
        }

        [Test]
        public void TwoPackageDownloadsShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithTwoPackageDownloads), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(2));

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4.5")));
        }

        [Test]
        public void PackageDownloadResultShouldBeReiterable()
        {
            var path = TempPath();

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(DirectoryBuildTargetsWithTwoPackageDownloads), path);

            foreach (var package in packages)
            {
                PackageAssert.IsPopulated(package);
            }

            Assert.That(packages.Select(p => p.Path), Is.All.EqualTo(path));
        }

        [Test]
        public void WhenOnePackageCannotBeRead_TheOthersAreStillRead_WithPackageDownload()
        {
            var badVersion = DirectoryBuildTargetsWithTwoPackageDownloads.Replace("1.2.3.4", "notaversion", StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(badVersion), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            PackageAssert.IsPopulated(packages[0]);
        }

        private static PackagePath TempPath()
        {
            return new PackagePath(
                OsSpecifics.GenerateBaseDirectory(),
                Path.Combine("src", "Directory.Build.Props"),
                PackageReferenceType.DirectoryBuildTargets);
        }

        private static DirectoryBuildTargetsReader MakeReader()
        {
            return new DirectoryBuildTargetsReader(Substitute.For<INuKeeperLogger>());
        }

        private static Stream StreamFromString(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
