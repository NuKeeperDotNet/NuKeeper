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

            var ff = new FolderFactory(new NullNuKeeperLogger());

            var result = reader.Read(ff.UniqueTemporaryFolder());

            Assert.That(result.Items.Count, Is.EqualTo(1));
            Assert.That(result.Items.First(), Is.EqualTo(NuGetSources.GlobalFeedUrl));
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
