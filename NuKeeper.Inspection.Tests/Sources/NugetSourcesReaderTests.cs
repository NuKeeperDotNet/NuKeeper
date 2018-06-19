using System.IO;
using System.Linq;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Sources;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Sources
{
    public class NugetSourcesReaderTests
    {
        [Test]
        public void OverrideSourcesAreUsedWhenSupplied()
        {
            var overrrideSources = new NuGetSources("overrideA");
            var reader = MakeNuGetSourcesReader(overrrideSources);

            var ff = new FolderFactory(new NullNuKeeperLogger());

            var result = reader.Read(ff.UniqueTemporaryFolder());

            Assert.That(result, Is.EqualTo(overrrideSources));
        }

        [Test]
        public void GlobalFeedIsUsedAsLastResort()
        {
            var reader = MakeNuGetSourcesReader(null);

            var result = reader.Read(TemporaryFolder());

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.First(), Is.EqualTo(NuGetSources.GlobalFeedUrl));
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
            var reader = MakeNuGetSourcesReader(null);

            var folder = TemporaryFolder();
            var path = Path.Join(folder.FullPath, "nuget.config");
            File.WriteAllText(path, ConfigFileContents);

            var result = reader.Read(folder);

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.First(), Is.EqualTo("https://fromFile1.com"));
        }


        [Test]
        public void SettingsOverridesConfigFile()
        {
            var reader = MakeNuGetSourcesReader(new NuGetSources("https://fromConfigA.com"));

            var folder = TemporaryFolder();
            var path = Path.Join(folder.FullPath, "nuget.config");
            File.WriteAllText(path, ConfigFileContents);

            var result = reader.Read(folder);

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.First(), Is.EqualTo("https://fromConfigA.com"));
        }

        private static IFolder TemporaryFolder()
        {
            var ff = new FolderFactory(new NullNuKeeperLogger());
            return ff.UniqueTemporaryFolder();
        }

        private static INugetSourcesReader MakeNuGetSourcesReader(NuGetSources fallbackSources)
        {
            var logger = new NullNuKeeperLogger();
            return new NugetSourcesReader(fallbackSources,
                new NugetConfigFileReader
                    (new NugetConfigFileParser(logger), logger), logger);
        }
    }
}
