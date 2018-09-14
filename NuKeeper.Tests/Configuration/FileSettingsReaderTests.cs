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
            Assert.That(data.MaxPr, Is.Null);
            Assert.That(data.MaxRepo, Is.Null);
            Assert.That(data.Verbosity, Is.Null);
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
            Assert.That(data.MaxPr, Is.Null);
            Assert.That(data.MaxRepo, Is.Null);
            Assert.That(data.Verbosity, Is.Null);
        }

        [Test]
        public void PopulatedConfigReturnsAllSettings()
        {
            const string configData = @"{
               ""age"":""3d"",
               ""api"":""http://api.com"",
               ""include"":""fred"",
               ""exclude"":""fish"",
               ""includeRepos"":""repoIn"",
               ""excludeRepos"":""repoOut"",
               ""label"": [ ""foo"", ""bar"" ],
               ""maxpr"": 42,
               ""maxRepo"": 12,
               ""verbosity"": ""Detailed""
}";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
            Assert.That(data.Include, Is.EqualTo("fred"));
            Assert.That(data.Exclude, Is.EqualTo("fish"));
            Assert.That(data.IncludeRepos, Is.EqualTo("repoIn"));
            Assert.That(data.ExcludeRepos, Is.EqualTo("repoOut"));

            Assert.That(data.Label.Length, Is.EqualTo(2));
            Assert.That(data.Label, Does.Contain("foo"));
            Assert.That(data.Label, Does.Contain("bar"));

            Assert.That(data.MaxPr, Is.EqualTo(42));
            Assert.That(data.MaxRepo, Is.EqualTo(12));
            Assert.That(data.Verbosity, Is.EqualTo(LogLevel.Detailed));
        }

        [Test]
        public void ConfigKeysAreCaseInsensitive()
        {
            const string configData = @"{
               ""Age"":""3d"",
               ""API"":""http://api.com"",
               ""iNClude"":""fred"",
               ""excludE"":""fish"",
               ""IncluDeRepoS"":""repo2"",
               ""label"": [""mark"" ],
               ""MAXrepo"":3,
               ""vErBoSiTy"": ""Q""
}";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
            Assert.That(data.Include, Is.EqualTo("fred"));
            Assert.That(data.Exclude, Is.EqualTo("fish"));
            Assert.That(data.IncludeRepos, Is.EqualTo("repo2"));
            Assert.That(data.Label.Length, Is.EqualTo(1));
            Assert.That(data.Label, Does.Contain("mark"));
            Assert.That(data.MaxRepo, Is.EqualTo(3));
            Assert.That(data.Verbosity, Is.EqualTo(LogLevel.Quiet));
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
