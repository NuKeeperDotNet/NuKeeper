using NSubstitute;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Files
{
    [TestFixture]
    public class FolderTests
    {
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
