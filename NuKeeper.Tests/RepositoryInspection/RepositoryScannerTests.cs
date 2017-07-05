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
        public void ValidDirectoryWorks()
        {
            var scanner = new RepositoryScanner();

            var results = scanner.FindAllNugetPackages(Path.GetTempPath());

            Assert.That(results, Is.Not.Null);
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
            var fullPath = typeof(RepositoryScanner).GetTypeInfo().Assembly.Location;
            var runDir = Path.GetDirectoryName(fullPath);

            var projectRootDir = Directory.GetParent(runDir).Parent.Parent.Parent;
            return projectRootDir.FullName;
        }
    }
}
