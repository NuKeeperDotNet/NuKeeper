using NuGet.Versioning;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NuKeeper.Integration.Tests.RepositoryInspection
{
    [TestFixture]
    public class RepositoryScannerTests : TestWithFailureLogging
    {
        const string SinglePackageInFile =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""foo"" version=""1.2.3"" targetFramework=""net45"" />
</packages>";

        const string Vs2017ProjectFileTemplateWithPackages =
            @"<Project>
  <ItemGroup>
<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
  </ItemGroup>
</Project>";

        private const string NuspecWithDependency =
            @"<package><metadata><dependencies>
<dependency id=""foo"" version=""3.3.3.5"" /></dependencies></metadata></package>";

        private const string DirectoryBuildProps =
            @"<Project>
  <ItemGroup>
    <PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
  </ItemGroup>
</Project>";

        private const string DirectoryBuildTargetsWithManyItemGroups =
            @"<Project>
  <ItemGroup>
    <PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include=""foo2"" Version=""3.2.1""></PackageReference>
  </ItemGroup>
</Project>";

        private IFolder _uniqueTemporaryFolder;

        [SetUp]
        public void Setup()
        {
            _uniqueTemporaryFolder = UniqueTemporaryFolder();
        }

        [TearDown]
        public void TearDown()
        {
            _uniqueTemporaryFolder.TryDelete();
        }

        [Test]
        public void ValidEmptyDirectoryWorks()
        {
            var scanner = MakeScanner();

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void FindsPackagesConfig()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "packages.config", SinglePackageInFile);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void CorrectItemInPackagesConfig()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "packages.config", SinglePackageInFile);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            var item = results.FirstOrDefault();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Id, Is.EqualTo("foo"));
            Assert.That(item.Version, Is.EqualTo(new NuGetVersion(1, 2, 3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.PackagesConfig));
        }

        [Test]
        public void FindsCsprojFile()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "sample.csproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsVbprojFile()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "sample.vbproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsFsprojFile()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "sample.fsproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsNuspec()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "sample.nuspec", NuspecWithDependency);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void CorrectItemInCsProjFile()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "sample.csproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            var item = results.FirstOrDefault();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Id, Is.EqualTo("foo"));
            Assert.That(item.Version, Is.EqualTo(new NuGetVersion(1, 2, 3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void CorrectItemInDirectoryBuildProps()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "Directory.Build.props", DirectoryBuildProps);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            var item = results.FirstOrDefault();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Id, Is.EqualTo("foo"));
            Assert.That(item.Version, Is.EqualTo(new NuGetVersion(1, 2, 3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void CorrectItemsInDirectoryBuildTargets()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "Directory.Build.targets", DirectoryBuildTargetsWithManyItemGroups);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            var item = results.FirstOrDefault();
            var item2 = results.Skip(1).FirstOrDefault();

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Id, Is.EqualTo("foo"));
            Assert.That(item.Version, Is.EqualTo(new NuGetVersion(1, 2, 3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
            Assert.That(item2, Is.Not.Null);
            Assert.That(item2.Id, Is.EqualTo("foo2"));
            Assert.That(item2.Version, Is.EqualTo(new NuGetVersion(3, 2, 1)));
            Assert.That(item2.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void CorrectItemsInPackagesProps()
        {
            var scanner = MakeScanner();

            WriteFile(_uniqueTemporaryFolder, "Packages.props", DirectoryBuildProps);

            var results = scanner.FindAllNuGetPackages(_uniqueTemporaryFolder);

            var item = results.FirstOrDefault();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Id, Is.EqualTo("foo"));
            Assert.That(item.Version, Is.EqualTo(new NuGetVersion(1, 2, 3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void SelfTest()
        {
            var scanner = MakeScanner();
            var baseFolder = new Folder(NukeeperLogger, GetOwnRootDir());

            var results = scanner.FindAllNuGetPackages(baseFolder);

            Assert.That(results, Is.Not.Null, "in folder" + baseFolder.FullPath);
            Assert.That(results, Is.Not.Empty, "in folder" + baseFolder.FullPath);

        }

        private static DirectoryInfo GetOwnRootDir()
        {
            // If the test is running on (real example)
            // "C:\Code\NuKeeper\NuKeeper.Tests\bin\Debug\netcoreapp1.1\NuKeeper.dll"
            // then the app root directory to scan is "C:\Code\NuKeeper\"
            // So go up four dir levels to the root
            // Self is a convenient source of a valid project to scan
            var fullPath = new Uri(typeof(RepositoryScanner).GetTypeInfo().Assembly.Location).LocalPath;
            var runDir = Path.GetDirectoryName(fullPath);

            var projectRootDir = Directory.GetParent(runDir).Parent.Parent.Parent;
            return projectRootDir;
        }

        private IFolder UniqueTemporaryFolder()
        {
            var folderFactory = new FolderFactory(NukeeperLogger);
            return folderFactory.UniqueTemporaryFolder();
        }

        private IRepositoryScanner MakeScanner()
        {
            var logger = NukeeperLogger;
            return new RepositoryScanner(
                new ProjectFileReader(logger),
                new PackagesFileReader(logger),
                new NuspecFileReader(logger),
                new DirectoryBuildTargetsReader(logger),
                new DirectoryExclusions());
        }

        private static void WriteFile(IFolder path, string fileName, string contents)
        {
            using (var file = File.CreateText(Path.Combine(path.FullPath, fileName)))
            {
                file.WriteLine(contents);
            }
        }
    }
}
