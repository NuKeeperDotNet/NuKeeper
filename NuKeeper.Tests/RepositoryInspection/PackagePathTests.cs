using NUnit.Framework;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class PackagePathTests
    {
        [Test]
        public void ConstructorShouldProduceExpectedSimplePropsForProjectFile()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "\\checkout1\\src\\myproj.csproj", 
                PackageReferenceType.ProjectFile);

            Assert.That(path.BaseDirectory, Is.EqualTo("c:\\temp\\somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo("checkout1\\src\\myproj.csproj"));
            Assert.That(path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void ConstructorShouldProduceExpectedSimplePropsForPackagesConfigFile()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "\\checkout1\\src\\packages.config",
                PackageReferenceType.PackagesConfig);

            Assert.That(path.BaseDirectory, Is.EqualTo("c:\\temp\\somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo("checkout1\\src\\packages.config"));
            Assert.That(path.PackageReferenceType, Is.EqualTo(PackageReferenceType.PackagesConfig));
        }

        [Test]
        public void ConstructorShouldProduceExpectedCalculatedProps()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "checkout1\\src\\myproj.csproj",
                PackageReferenceType.ProjectFile);


            Assert.That(path.FullDirectory, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src"));
            Assert.That(path.FullPath, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src\\myproj.csproj"));
            Assert.That(path.FileName, Is.EqualTo("myproj.csproj"));
        }

        [Test]
        public void ConstructorShouldProduceExpectedCalculatedPropsWithExtraSlash()
        {
            var path = new PackagePath("c:\\temp\\somefolder", "\\checkout1\\src\\myproj.csproj",
                PackageReferenceType.ProjectFile);


            Assert.That(path.BaseDirectory, Is.EqualTo("c:\\temp\\somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo("checkout1\\src\\myproj.csproj"));
            Assert.That(path.FullDirectory, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src"));
            Assert.That(path.FullPath, Is.EqualTo("c:\\temp\\somefolder\\checkout1\\src\\myproj.csproj"));
            Assert.That(path.FileName, Is.EqualTo("myproj.csproj"));
        }
    }
}
