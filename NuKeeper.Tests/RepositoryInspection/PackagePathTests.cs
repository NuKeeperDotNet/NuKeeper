using NUnit.Framework;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class PackagePathTests
    {
        [Test]
        public void ConstructorShouldProduceExpectedSimpleProps()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "\\checkout1\\src\\myproj.csproj");

            Assert.That(path.BaseDirectory, Is.EqualTo("c:\\temp\\somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo("checkout1\\src\\myproj.csproj"));
        }

        [Test]
        public void ConstructorShouldProduceExpectedCalculatedProps()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "checkout1\\src\\myproj.csproj");


            Assert.That(path.FullDirectory, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src"));
            Assert.That(path.FullPath, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src\\myproj.csproj"));
            Assert.That(path.FileName, Is.EqualTo("myproj.csproj"));
        }

        [Test]
        public void ConstructorShouldProduceExpectedCalculatedPropsWithExtraSlash()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "\\checkout1\\src\\myproj.csproj");


            Assert.That(path.BaseDirectory, Is.EqualTo("c:\\temp\\somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo("checkout1\\src\\myproj.csproj"));
            Assert.That(path.FullDirectory, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src"));
            Assert.That(path.FullPath, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src\\myproj.csproj"));
            Assert.That(path.FileName, Is.EqualTo("myproj.csproj"));
        }
    }
}
