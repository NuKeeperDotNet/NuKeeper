using System.IO;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;

namespace NuKeeper.Tests.Configuration
{
    [TestFixture]
    public class FileSettingsReaderTests
    {
        [Test]
        public void MissingFileReturnsNoSettings()
        {
            var folder = TemporaryFolder();

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(folder.FullPath);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.Null);
            Assert.That(data.Api, Is.Null);
            Assert.That(data.Include, Is.Null);
            Assert.That(data.Exclude, Is.Null);
            Assert.That(data.Label, Is.Null);
        }

        [Test]
        public void EmptyConfigReturnsNoSettings()
        {
            var path = MakeTestFile("{}");

            var fsr =  new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.Null);
            Assert.That(data.Api, Is.Null);
            Assert.That(data.Include, Is.Null);
            Assert.That(data.Exclude, Is.Null);
            Assert.That(data.Label, Is.Null);
        }

        [Test]
        public void PopulatedConfigReturnsAllSettings()
        {
            const string configData = @"{
               ""age"":""3d"",
               ""api"":""http://api.com"",
               ""include"":""fred"",
               ""exclude"":""fish"",
               ""label"":""mark""
}";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
            Assert.That(data.Include, Is.EqualTo("fred"));
            Assert.That(data.Exclude, Is.EqualTo("fish"));
            Assert.That(data.Label, Is.EqualTo("mark"));
        }

        [Test]
        public void ConfigKeysAreCaseInsensitive()
        {
            const string configData = @"{
               ""Age"":""3d"",
               ""API"":""http://api.com"",
               ""iNClude"":""fred"",
               ""excludE"":""fish"",
               ""label"":""mark""
}";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
            Assert.That(data.Include, Is.EqualTo("fred"));
            Assert.That(data.Exclude, Is.EqualTo("fish"));
            Assert.That(data.Label, Is.EqualTo("mark"));
        }

        [Test]
        public void ExtraKeysAreIgnored()
        {
            const string configData = @"{
               ""age"":""3d"",
               ""api"":""http://api.com"",
               ""something"":""nothing""
}";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
        }


        private string MakeTestFile(string contents)
        {
            var folder = TemporaryFolder();
            var path = Path.Join(folder.FullPath, "nukeeper.settings.json");
            File.WriteAllText(path, contents);
            return folder.FullPath;
        }

        private static IFolder TemporaryFolder()
        {
            var ff = new FolderFactory(Substitute.For<INuKeeperLogger>());
            return ff.UniqueTemporaryFolder();
        }

    }
}
