using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Versioning;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.RepositoryInspection
{
    [TestFixture]
    public class DirectoryBuildTargetsReaderTests
    {
        const string PackagesFileWithSinglePackage =
            @"<Project><ItemGroup><PackageReference Include=""foo"" Version=""1.2.3.4"" /></ItemGroup></Project>";
        const string GlobalPackageReferenceFileWithSinglePackage =
            @"<Project><ItemGroup><GlobalPackageReference Include=""foo"" Version=""1.2.3.4"" /></ItemGroup></Project>";
        const string SdkFileWithSinglePackage =
            @"<Project><Sdk Name=""foo"" Version=""1.2.3.4"" /></Project>";

        private const string PackagesFileWithTwoPackages = @"<Project><ItemGroup>
<PackageReference Include=""foo"" Version=""1.2.3.4"" />
<PackageReference Update=""bar"" Version=""2.3.4.5"" /></ItemGroup></Project>";
        private IFolder _uniqueTemporaryFolder = null;

        [SetUp]
        public void Setup()
        {
            _uniqueTemporaryFolder = TemporaryFolder();
        }

        [TearDown]
        public void TearDown()
        {
            _uniqueTemporaryFolder.TryDelete();
        }

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
        public void SinglePackageShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithSinglePackage), TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithSinglePackage), TempPath());

            var package = packages.FirstOrDefault();
            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SinglePackageShouldBeCorrect()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithSinglePackage), TempPath());

            var package = packages.FirstOrDefault();

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void SingleGlobalPackageReferenceShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(GlobalPackageReferenceFileWithSinglePackage), TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SingleGlobalPackageReferenceShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(GlobalPackageReferenceFileWithSinglePackage), TempPath());

            var package = packages.FirstOrDefault();
            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SingleGlobalPackageReferenceShouldBeCorrect()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(GlobalPackageReferenceFileWithSinglePackage), TempPath());

            var package = packages.FirstOrDefault();

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void SingleSdkShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(SdkFileWithSinglePackage), TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SingleSdkShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(SdkFileWithSinglePackage), TempPath());

            var package = packages.FirstOrDefault();
            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SingleSdkShouldBeCorrect()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(SdkFileWithSinglePackage), TempPath());

            var package = packages.FirstOrDefault();

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void TwoPackagesShouldBePopulated()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithTwoPackages), TempPath())
                .ToList();

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages.Count, Is.EqualTo(2));

            PackageAssert.IsPopulated(packages[0]);
            PackageAssert.IsPopulated(packages[1]);
        }

        [Test]
        public void TwoPackagesShouldBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithTwoPackages), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(2));

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4.5")));
        }

        [Test]
        public void ResultIsReiterable()
        {
            var path = TempPath();

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithTwoPackages), path);

            foreach (var package in packages)
            {
                PackageAssert.IsPopulated(package);
            }

            Assert.That(packages.Select(p => p.Path), Is.All.EqualTo(path));
        }

        [Test]
        public void ThreePackagesShouldBePopulatedByImport()
        {
            var temp = Path.Combine(_uniqueTemporaryFolder.FullPath, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + ".props");
            File.WriteAllText(temp, PackagesFileWithTwoPackages);
            var PackagesFileWithImport = $@"<Project>
<Import Project=""{Path.GetRelativePath(_uniqueTemporaryFolder.FullPath, temp)}"" />
<ItemGroup>
<PackageReference Update=""file1"" Version=""2.3.4"" />
</ItemGroup></Project>";
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithImport), TempPath())
                .ToList();

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages.Count, Is.EqualTo(3));

            PackageAssert.IsPopulated(packages[0]);
            PackageAssert.IsPopulated(packages[1]);
            PackageAssert.IsPopulated(packages[2]);
        }

        [Test]
        public void ThreePackagesShouldBeReadByImportBeRead()
        {
            var temp = Path.Combine(_uniqueTemporaryFolder.FullPath, Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture) + ".props");
            File.WriteAllText(temp, PackagesFileWithTwoPackages);
            var PackagesFileWithImport = $@"<Project>
<Import Project=""{Path.GetRelativePath(_uniqueTemporaryFolder.FullPath, temp)}"" />
<ItemGroup>
<PackageReference Update=""file1"" Version=""2.3.4"" />
</ItemGroup></Project>";
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(PackagesFileWithImport), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(3));

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4.5")));

            Assert.That(packages[2].Id, Is.EqualTo("file1"));
            Assert.That(packages[2].Version, Is.EqualTo(new NuGetVersion("2.3.4")));
        }

        [Test]
        public void WhenOnePackageCannotBeRead_TheOthersAreStillRead()
        {
            var badVersion = PackagesFileWithTwoPackages.Replace("1.2.3.4", "notaversion", StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(badVersion), TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            PackageAssert.IsPopulated(packages[0]);
        }

        private PackagePath TempPath()
        {
            return new PackagePath(
                _uniqueTemporaryFolder.FullPath,
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

        private static IFolder TemporaryFolder()
        {
            var ff = new FolderFactory(Substitute.For<INuKeeperLogger>());
            return ff.UniqueTemporaryFolder();
        }
    }
}
