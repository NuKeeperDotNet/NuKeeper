using NuKeeper.Inspection.Sources;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.Sources
{
    [TestFixture]
    public class NuGetSourcesTests
    {
        [Test]
        public void ShouldGenerateCommandLineArguments()
        {
            var subject = new NuGetSources("one", "two");

            var result = subject.CommandLine("-s");

            Assert.AreEqual("-s one -s two", result);
        }

        [Test]
        public void ShouldEscapeLocalPaths()
        {
            var subject = new NuGetSources("file://one", "C:/Program Files (x86)/Microsoft SDKs/NuGetPackages/", "http://two");

            var result = subject.CommandLine("-s");

            Assert.AreEqual("-s file://one -s \"C:/Program Files (x86)/Microsoft SDKs/NuGetPackages/\" -s http://two", result);
        }
    }
}
