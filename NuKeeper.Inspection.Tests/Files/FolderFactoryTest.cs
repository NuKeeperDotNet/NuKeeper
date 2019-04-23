using System;
using System.IO;
using System.Linq;
using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Inspection.Files;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Files
{
    [TestFixture]
    public class FolderFactoryTest
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

        private void ClearTemp()
        {
            var path = FolderFactory.NuKeeperTempFilesPath();
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }

        [Test]
        public void OnlySelectTempFoldersOlderThanOneHour()
        {
            var factory = new FolderFactory(Substitute.For<INuKeeperLogger>());

            // set up edge cases
            var folder1 = factory.UniqueTemporaryFolder();
            Directory.SetLastWriteTime(folder1.FullPath, DateTime.Now.AddHours(-1).AddMinutes(-1));
            var folder2 = factory.UniqueTemporaryFolder();
            Directory.SetLastWriteTime(folder2.FullPath, DateTime.Now.AddHours(-1).AddMinutes(1));

            var baseFolder = Path.GetDirectoryName(folder1.FullPath);
            var baseDirInfo = new DirectoryInfo(baseFolder);

            var toDelete = FolderFactory.GetTempDirsToCleanup(baseDirInfo).ToArray();

            Assert.AreEqual(1, toDelete.Length, "Only 1 folder should be marked for deletion");
            Assert.AreEqual(folder1.FullPath, toDelete[0].FullName, "wrong folder marked for deletion");
        }

        [Test]
        public void OnlySelectTempFoldersWithPrefix()
        {
            var factory = new FolderFactory(Substitute.For<INuKeeperLogger>());

            // set up edge cases
            var folder1 = factory.UniqueTemporaryFolder();
            Directory.SetLastWriteTime(folder1.FullPath, DateTime.Now.AddHours(-2));

            var baseFolder = Path.GetDirectoryName(folder1.FullPath);
            var baseDirInfo = new DirectoryInfo(baseFolder);
            var notToToDeletePath = Path.Combine(baseFolder, "tools");
            Directory.CreateDirectory(notToToDeletePath);
            Directory.SetLastWriteTime(notToToDeletePath, DateTime.Now.AddHours(-2));

            var toDelete = FolderFactory.GetTempDirsToCleanup(baseDirInfo).ToArray();

            Assert.AreEqual(1, toDelete.Length, "Only 1 folder should be marked for deletion");
            Assert.AreEqual(folder1.FullPath, toDelete[0].FullName, "wrong folder marked for deletion");
        }
    }
}
