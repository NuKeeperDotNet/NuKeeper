using System;
using System.IO;
using System.Reflection;
using NuKeeper.Files;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class RepositoryScannerTests
    {
        [Test]
        public void InvalidDirectoryThrows()
        {
            var scanner = new RepositoryScanner();

            Assert.Throws<Exception>(() => scanner.FindAllNuGetPackages("fish"));
        }

        [Test]
        public void ValidEmptyDirectoryWorks()
        {
            var scanner = new RepositoryScanner();

            var results = scanner.FindAllNuGetPackages(TempFiles.MakeUniqueTemporaryPath());

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

            var scanner = new RepositoryScanner();

            var temporaryPath = TempFiles.MakeUniqueTemporaryPath();

            using (var file = File.CreateText(Path.Combine(temporaryPath, "packages.config")))
            {
                file.WriteLine(singlePackage);
            }

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

            var scanner = new RepositoryScanner();

            var temporaryPath = TempFiles.MakeUniqueTemporaryPath();

            using (var file = File.CreateText(Path.Combine(temporaryPath, "sample.csproj")))
            {
                file.WriteLine(Vs2017ProjectFileTemplateWithPackages);
            }

            var results = scanner.FindAllNuGetPackages(temporaryPath);

            Assert.That(results, Has.Count.EqualTo(1));
        }

        [Test]
        public void SelfTest()
        {
            var basePath = GetOwnRootDir();

            var scanner = new RepositoryScanner();

            var results = scanner.FindAllNuGetPackages(basePath);

            Assert.That(results, Is.Not.Null, "in folder" + basePath);
            Assert.That(results, Is.Not.Empty, "in folder" + basePath);

        }

        private static string GetOwnRootDir()
        {
            // If the test is running on (real example)
            // "C:\Code\NuKeeper\NuKeeper.Tests\bin\Debug\netcoreapp1.1\NuKeeper.dll"
            // then the app root directory to scan is "C:\Code\NuKeeper\"
            // So go up four dir levels to the root
            // Self is a convenient source of a valid project to scan
            var fullPath = new Uri(typeof(RepositoryScanner).GetTypeInfo().Assembly.CodeBase).LocalPath;
            var runDir = Path.GetDirectoryName(fullPath);

            var projectRootDir = Directory.GetParent(runDir).Parent.Parent.Parent;
            return projectRootDir.FullName;
        }
    }
}
