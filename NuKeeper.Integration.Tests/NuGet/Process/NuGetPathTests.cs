using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class NuGetPathTests : BaseTest
    {
        [Test]
        public void HasNugetPath()
        {
            var nugetPath = new NuGetPath(NukeeperLogger).Executable;

            Assert.That(nugetPath, Is.Not.Empty);
            FileAssert.Exists(nugetPath);
        }
    }
}
