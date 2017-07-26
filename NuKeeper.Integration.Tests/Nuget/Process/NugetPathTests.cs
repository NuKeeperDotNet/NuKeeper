using NuKeeper.Nuget.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.Nuget.Process
{
    [TestFixture]
    public class NugetPathTests
    {
        [Test]
        public void HasNugetPath()
        {
            var nugetPath = NugetPath.Find();

            Assert.That(nugetPath, Is.Not.Empty);
            FileAssert.Exists(nugetPath);
        }
    }
}
