using System;
using System.IO;
using System.Reflection;
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

            Assert.Throws<Exception>(() => scanner.FindAllNugetPackages("fish"));
        }

        [Test]
        public void ValidEmptyDirectoryWorks()
        {
            var scanner = new RepositoryScanner();

            var results = scanner.FindAllNugetPackages(FileHelper.MakeUniqueTemporaryPath());

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void SelfTest()
        {
            var basePath = GetOwnRootDir();

            var scanner = new RepositoryScanner();

            var results = scanner.FindAllNugetPackages(basePath);

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
            var fullPath = typeof(RepositoryScanner).GetTypeInfo().Assembly.Location;
            var runDir = Path.GetDirectoryName(fullPath);

            var projectRootDir = Directory.GetParent(runDir).Parent.Parent.Parent;
            return projectRootDir.FullName;
        }
    }
}
