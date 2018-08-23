using NSubstitute;
using NuKeeper.Inspection.Logging;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class NuGetPathTests
    {
        [Test]
        public void HasNugetPath()
        {
            var nugetPath = new NuGetPath(Substitute.For<INuKeeperLogger>()).Executable;

            Assert.That(nugetPath, Is.Not.Empty);
            FileAssert.Exists(nugetPath);
        }
    }
}
