using NuKeeper.Inspection.Sources;
using NUnit.Framework;
using System.Linq;

namespace NuKeeper.Inspection.Tests.Sources
{
    [TestFixture]
    public class NugetConfigFileParserTests
    {
        [Test]
        public void EmptyStringReturnsNull()
        {
            var data = string.Empty;

            var parser = new NugetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(data);

            Assert.That(sources, Is.Null);
        }

        [Test]
        public void InvalidStringReturnsNull()
        {
            var data = "not valid markup";

            var parser = new NugetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(data);

            Assert.That(sources, Is.Null);
        }

        [Test]
        public void ValidDataWithOneItemReturnsThatItem()
        {
            var data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""NuGet official"" value=""https://nuget.org/api/v2/"" />
  </packageSources>
</configuration>";

            var parser = new NugetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(data);

            Assert.That(sources, Is.Not.Null);
            Assert.That(sources.Items.Count, Is.EqualTo(1));
            Assert.That(sources.Items.First(), Is.EqualTo("https://nuget.org/api/v2/"));
        }

        [Test]
        public void ValidDataWithTwoItemsReturnsBoth()
        {
            var data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""NuGet official"" value=""https://nuget.org/api/v2/"" />
    <add key=""Custom"" value=""https://some.other.feed"" />
  </packageSources>
</configuration>";

            var parser = new NugetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(data);

            Assert.That(sources, Is.Not.Null);
            Assert.That(sources.Items.Count, Is.EqualTo(2));
        }

        [Test]
        public void InvalidSourcesAreOmitted()
        {
            var data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""NuGet official"" value=""https://nuget.org/api/v2/"" />
    <add key=""Custom"" />
  </packageSources>
</configuration>";

            var parser = new NugetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(data);

            Assert.That(sources, Is.Not.Null);
            Assert.That(sources.Items.Count, Is.EqualTo(1));
        }
    }
}
