using System.IO;
using NSubstitute;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.Files;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Files
{
    [TestFixture]
    public class FolderTests
    {
        [SetUp]
        public void Setup()
        {
            ClearTemp();
        }

        [TearDown]
        public void TearDown()
        {
            ClearTemp();
        }

        private static void ClearTemp()
        {
            var path = FolderFactory.NuKeeperTempFilesPath();
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        [Test]
        public void FolderExists()
        {
            var folder = MakeTempFolder();

            Assert.That(folder.FullPath, Is.Not.Empty);
            DirectoryAssert.Exists(folder.FullPath);
        }

        [Test]
        public void AfterDeleteFolderDoesNotExist()
        {
            var folder = MakeTempFolder();

            DirectoryAssert.Exists(folder.FullPath);
            folder.TryDelete();
            DirectoryAssert.DoesNotExist(folder.FullPath);
        }

        private static IFolder MakeTempFolder()
        {
            var factory = new FolderFactory(Substitute.For<INuKeeperLogger>());
            return factory.UniqueTemporaryFolder();
        }
    }
}
