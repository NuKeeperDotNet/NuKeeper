using NuKeeper.NuGet.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture, Category("WindowsOnly")]
    public class NuGetPathTests
    {
        [Test]
        public void HasNugetPath()
        {
            var nugetPath = NuGetPath.FindExecutable();

            Assert.That(nugetPath, Is.Not.Empty);
            FileAssert.Exists(nugetPath);
        }
    }
}
