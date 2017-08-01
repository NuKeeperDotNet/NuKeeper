using NuKeeper.NuGet.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture, Category("WindowsOnly")]
    public class NugetPathTests
    {
        [Test]
        public void HasNugetPath()
        {
            var nugetPath = NugetPath.FindExecutable();

            Assert.That(nugetPath, Is.Not.Empty);
            FileAssert.Exists(nugetPath);
        }
    }
}
