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
        [Test]
        public void OnlySelectTempFoldersOlderThanOneHour()
        {
            var factory = new FolderFactory(Substitute.For<INuKeeperLogger>());

            // set up edge cases
            var folder1 = factory.UniqueTemporaryFolder();
            Directory.SetLastWriteTime(folder1.FullPath, DateTime.Now.AddHours(-1).AddMinutes(-1));
            var folder2 = factory.UniqueTemporaryFolder();
            Directory.SetLastWriteTime(folder2.FullPath, DateTime.Now.AddHours(-1).AddMinutes(1));

            var baseDirInfo = new DirectoryInfo(FolderFactory.NuKeeperTempFilesPath());

            var toDelete = FolderFactory.GetTempDirsToCleanup(baseDirInfo).ToArray();

            Assert.AreEqual(1, toDelete.Length, "Only 1 folder should be marked for deletion");
            Assert.AreEqual(folder1.FullPath, toDelete[0].FullName, "wrong folder marked for deletion");

            folder1.TryDelete();
            folder2.TryDelete();
        }

        [Test]
        public void OnlySelectTempFoldersWithPrefix()
        {
            var factory = new FolderFactory(Substitute.For<INuKeeperLogger>());

            // set up edge cases
            var folder1 = factory.UniqueTemporaryFolder();
            Directory.SetLastWriteTime(folder1.FullPath, DateTime.Now.AddHours(-2));
            var notToToDeletePath = Path.Combine(FolderFactory.NuKeeperTempFilesPath(), "tools");
            Directory.CreateDirectory(notToToDeletePath);
            Directory.SetLastWriteTime(notToToDeletePath, DateTime.Now.AddHours(-2));

            var baseDirInfo = new DirectoryInfo(FolderFactory.NuKeeperTempFilesPath());

            var toDelete = FolderFactory.GetTempDirsToCleanup(baseDirInfo).ToArray();

            Assert.AreEqual(1, toDelete.Length, "Only 1 folder should be marked for deletion");
            Assert.AreEqual(folder1.FullPath, toDelete[0].FullName, "wrong folder marked for deletion");

            folder1.TryDelete();
            if (Directory.Exists(notToToDeletePath))
            {
                Directory.Delete(notToToDeletePath);
            }
        }
    }
}
