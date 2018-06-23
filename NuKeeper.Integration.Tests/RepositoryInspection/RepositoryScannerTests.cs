using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NuGet.Versioning;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Integration.Tests.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.RepositoryInspection
{
    [TestFixture]
    public class RepositoryScannerTests
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

        [Test]
        public void ValidEmptyDirectoryWorks()
        {
            var scanner = MakeScanner();

            var results = scanner.FindAllNuGetPackages(GetUniqueTempFolder());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void FindsPackagesConfig()
        {
            var scanner = MakeScanner();

            var temporaryPath = GetUniqueTempFolder();
            WriteFile(temporaryPath, "packages.config", SinglePackageInFile);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void CorrectItemInPackagesConfig()
        {
            var scanner = MakeScanner();

            var temporaryPath = GetUniqueTempFolder();
            WriteFile(temporaryPath, "packages.config", SinglePackageInFile);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            var item = results.FirstOrDefault();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Identity.Id, Is.EqualTo("foo"));
            Assert.That(item.Identity.Version, Is.EqualTo(new NuGetVersion(1,2,3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.PackagesConfig));
        }

        [Test]
        public void FindsCsprojFile()
        {
            var scanner = MakeScanner();
            var temporaryPath = GetUniqueTempFolder();

            WriteFile(temporaryPath, "sample.csproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsVbprojFile()
        {
            var scanner = MakeScanner();
            var temporaryPath = GetUniqueTempFolder();

            WriteFile(temporaryPath, "sample.vbproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsFsprojFile()
        {
            var scanner = MakeScanner();
            var temporaryPath = GetUniqueTempFolder();

            WriteFile(temporaryPath, "sample.fsproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsNuspec()
        {
            var scanner = MakeScanner();
            var temporaryPath = GetUniqueTempFolder();

            WriteFile(temporaryPath, "sample.nuspec", NuspecWithDependency);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void CorrectItemInCsProjFile()
        {
            var scanner = MakeScanner();
            var temporaryPath = GetUniqueTempFolder();

            WriteFile(temporaryPath, "sample.csproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            var item = results.FirstOrDefault();

            Assert.That(item, Is.Not.Null);
            Assert.That(item.Identity.Id, Is.EqualTo("foo"));
            Assert.That(item.Identity.Version, Is.EqualTo(new NuGetVersion(1, 2, 3)));
            Assert.That(item.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void SelfTest()
        {
            var scanner = MakeScanner();
            var baseFolder = new Folder(new NullNuKeeperLogger(), GetOwnRootDir());

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
            var fullPath = new Uri(typeof(RepositoryScanner).GetTypeInfo().Assembly.CodeBase).LocalPath;
            var runDir = Path.GetDirectoryName(fullPath);

            var projectRootDir = Directory.GetParent(runDir).Parent.Parent.Parent;
            return projectRootDir;
        }

        private IFolder GetUniqueTempFolder()
        {
            var folderFactory = new FolderFactory(new NullNuKeeperLogger());
            return folderFactory.UniqueTemporaryFolder();
        }

        private IRepositoryScanner MakeScanner()
        {
            var logger = new NullNuKeeperLogger();
            return new RepositoryScanner(
                new ProjectFileReader(logger),
                new PackagesFileReader(logger),
                new NuspecFileReader(logger));
        }

        private void WriteFile(IFolder path, string fileName, string contents)
        {
            using (var file = File.CreateText(Path.Combine(path.FullPath, fileName)))
            {
                file.WriteLine(contents);
            }
        }
    }
}
