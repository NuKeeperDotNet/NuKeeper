using NuKeeper.Inspection.Sources;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Text;

namespace NuKeeper.Inspection.Tests.Sources
{
    [TestFixture]
    public class NugetConfigFileParserTests
    {
        [Test]
        public void EmptyStringReturnsNull()
        {
            var data = string.Empty;

            var parser = new NuGetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(ToStream(data));

            Assert.That(sources, Is.Null);
        }

        [Test]
        public void InvalidStringReturnsNull()
        {
            var data = "not valid markup";

            var parser = new NuGetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(ToStream(data));

            Assert.That(sources, Is.Null);
        }

        [Test]
        public void ValidDataWithOneItemReturnsThatItem()
        {
            var data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""NuGet official"" value=""https://api.nuget.org/v3/index.json"" />
  </packageSources>
</configuration>";

            var parser = new NuGetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(ToStream(data));

            Assert.That(sources, Is.Not.Null);
            Assert.That(sources.Items.Count, Is.EqualTo(1));
            Assert.That(sources.Items.First(), Is.EqualTo(NuGetSources.GlobalFeedUrl));
        }

        [Test]
        public void ValidDataWithTwoItemsReturnsBoth()
        {
            var data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""NuGet official"" value=""https://api.nuget.org/v3/index.json"" />
    <add key=""Custom"" value=""https://some.other.feed"" />
  </packageSources>
</configuration>";

            var parser = new NuGetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(ToStream(data));

            Assert.That(sources, Is.Not.Null);
            Assert.That(sources.Items.Count, Is.EqualTo(2));
            Assert.That(sources.Items.First(), Is.EqualTo(NuGetSources.GlobalFeedUrl));
            Assert.That(sources.Items.Last(), Is.EqualTo("https://some.other.feed"));
        }

        [Test]
        public void InvalidSourcesAreOmitted()
        {
            var data =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <add key=""NuGet official"" value=""https://api.nuget.org/v3/index.json"" />
    <add key=""Custom"" />
  </packageSources>
</configuration>";

            var parser = new NuGetConfigFileParser(new NullNuKeeperLogger());

            var sources = parser.Parse(ToStream(data));

            Assert.That(sources, Is.Not.Null);
            Assert.That(sources.Items.Count, Is.EqualTo(1));
            Assert.That(sources.Items.First(), Is.EqualTo(NuGetSources.GlobalFeedUrl));
        }

        private static Stream ToStream(string str, Encoding enc = null)
        {
            enc = enc ?? Encoding.UTF8;
            return new MemoryStream(enc.GetBytes(str ?? ""));
        }
    }
}
