using NuKeeper.Engine.FilesUpdate;
using NuKeeper.Files;
using NuKeeper.Integration.Tests.NuGet.Api;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Engine.FilesUpdate
{
    [TestFixture]
    public class ConfigFileFinderTests
    {
        [Test]
        public void NoConfigFilesFoundInNewFolder()
        {
            var finder = new ConfigFileFinder();
            var files = finder.FindInFolder(MakeTempFolder());

            Assert.That(files, Is.Not.Null);
            Assert.That(files, Is.Empty);
        }

        private static IFolder MakeTempFolder()
        {
            var factory = new FolderFactory(new NullNuKeeperLogger());
            return factory.UniqueTemporaryFolder();
        }
    }
}
