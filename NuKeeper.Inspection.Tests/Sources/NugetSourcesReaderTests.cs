using System.IO;
using System.Linq;
using NSubstitute;
using NuGet.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Sources;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Sources
{
    public class NugetSourcesReaderTests
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
        public void OverrideSourcesAreUsedWhenSupplied()
        {
            var overrrideSources = new NuGetSources("overrideA");
            var reader = MakeNuGetSourcesReader();

            var ff = new FolderFactory(Substitute.For<INuKeeperLogger>());

            var result = reader.Read(ff.UniqueTemporaryFolder(), overrrideSources);

            Assert.That(result, Is.EqualTo(overrrideSources));
        }

        [Test]
        public void GlobalFeedIsUsedAsLastResort()
        {
            var reader = MakeNuGetSourcesReader();

            var result = reader.Read(TemporaryFolder(), null);

            Assert.That(result.Items.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Items, Does.Contain(new PackageSource("https://api.nuget.org/v3/index.json", "nuget.org")));
        }

        private const string ConfigFileContents =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""From A file"" value=""https://fromFile1.com"" />
  </packageSources>
</configuration>";

        [Test]
        public void ConfigFileIsUsed()
        {
            var reader = MakeNuGetSourcesReader();

            var folder = TemporaryFolder();
            var path = Path.Join(folder.FullPath, "nuget.config");
            File.WriteAllText(path, ConfigFileContents);

            var result = reader.Read(folder, null);

            Assert.That(result.Items.Count, Is.GreaterThanOrEqualTo(1));
            Assert.That(result.Items.First(), Is.EqualTo(new PackageSource("https://fromFile1.com", "From A file")));
        }


        [Test]
        public void SettingsOverridesConfigFile()
        {
            var reader = MakeNuGetSourcesReader();

            var folder = TemporaryFolder();
            var path = Path.Join(folder.FullPath, "nuget.config");
            File.WriteAllText(path, ConfigFileContents);

            var result = reader.Read(folder, new NuGetSources("https://fromConfigA.com"));

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.First(), Is.EqualTo(new PackageSource("https://fromConfigA.com")));
        }

        private static IFolder TemporaryFolder()
        {
            var ff = new FolderFactory(Substitute.For<INuKeeperLogger>());
            return ff.UniqueTemporaryFolder();
        }

        private static INuGetSourcesReader MakeNuGetSourcesReader()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            return new NuGetSourcesReader(
                new NuGetConfigFileReader
                    (logger), logger);
        }
    }
}
