using System;
using System.IO;
using System.Reflection;
using NuKeeper.Files;
using NuKeeper.Integration.Tests.NuGet.Api;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.RepositoryInspection
{
    [TestFixture]
    public class RepositoryScannerTests
    {
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
            const string singlePackage =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<packages>
  <package id=""foo"" version=""1.2.3.4"" targetFramework=""net45"" />
</packages>";

            var scanner = MakeScanner();

            var temporaryPath = GetUniqueTempFolder();
            WriteFile(temporaryPath, "packages.config", singlePackage);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void FindsCsprojFile()
        {
            const string Vs2017ProjectFileTemplateWithPackages =
                @"<Project>
  <ItemGroup>
<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
  </ItemGroup>
</Project>";

            var scanner = MakeScanner();
            var temporaryPath = GetUniqueTempFolder();

            WriteFile(temporaryPath, "sample.csproj", Vs2017ProjectFileTemplateWithPackages);

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
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
                new PackagesFileReader(logger));
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
