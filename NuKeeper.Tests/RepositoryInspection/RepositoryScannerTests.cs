using System;
using System.IO;
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
            var scanner = new RepositoryScanner();

            var results = scanner.FindAllNugetPackages("C:\\Code\\NuKeeper");

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Not.Empty);

        }
    }
}
