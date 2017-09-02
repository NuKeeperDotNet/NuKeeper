using System.IO;
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
            var sep = Path.DirectorySeparatorChar;

            var path = new PackagePath(
                $"c:{sep}temp{sep}somefolder", 
                $"{sep}checkout1{sep}src{sep}myproj.csproj", 
                PackageReferenceType.ProjectFile);

            Assert.That(path.BaseDirectory, Is.EqualTo($"c:{sep}temp{sep}somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo($"checkout1{sep}src{sep}myproj.csproj"));
            Assert.That(path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void ConstructorShouldProduceExpectedSimplePropsForPackagesConfigFile()
        {
            var sep = Path.DirectorySeparatorChar;
            var path = new PackagePath(
                $"c:{sep}temp{sep}somefolder", 
                $"{sep}checkout1{sep}src{sep}packages.config",
                PackageReferenceType.PackagesConfig);

            Assert.That(path.BaseDirectory, Is.EqualTo($"c:{sep}temp{sep}somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo($"checkout1{sep}src{sep}packages.config"));
            Assert.That(path.PackageReferenceType, Is.EqualTo(PackageReferenceType.PackagesConfig));
        }

        [Test]
        public void ConstructorShouldProduceExpectedCalculatedProps()
        {
            var sep = Path.DirectorySeparatorChar;
            var path = new PackagePath(
                $"c:{sep}temp{sep}somefolder", 
                $"checkout1{sep}src{sep}myproj.csproj",
                PackageReferenceType.ProjectFile);


            Assert.That(path.FullDirectory, Is.EqualTo($"c:{sep}temp{sep}somefolder{sep}checkout1{sep}src"));
            Assert.That(path.FullPath, Is.EqualTo($"c:{sep}temp{sep}somefolder{sep}checkout1{sep}src{sep}myproj.csproj"));
            Assert.That(path.FileName, Is.EqualTo("myproj.csproj"));
        }

        [Test]
        public void ConstructorShouldProduceExpectedInfoForProjectFile()
        {
            var sep = Path.DirectorySeparatorChar;

            var path = new PackagePath(
                $"c:{sep}temp{sep}somefolder",
                $"{sep}checkout1{sep}src{sep}myproj.csproj",
                PackageReferenceType.ProjectFile);

            Assert.That(path.Info, Is.Not.Null);
            Assert.That(path.Info.Name, Is.EqualTo("myproj.csproj"));
            Assert.That(path.Info.DirectoryName, Is.EqualTo($"c:{sep}temp{sep}somefolder{sep}checkout1{sep}src"));
            Assert.That(path.Info.FullName, Is.EqualTo($"c:{sep}temp{sep}somefolder{sep}checkout1{sep}src{sep}myproj.csproj"));
        }

        [Test]
        public void ConstructorShouldProduceExpectedCalculatedPropsWithExtraSlash()
        {
            var sep = Path.DirectorySeparatorChar;
            var path = new PackagePath(
                $"c:{sep}temp{sep}somefolder", 
                $"{sep}checkout1{sep}src{sep}myproj.csproj",
                PackageReferenceType.ProjectFile);


            Assert.That(path.BaseDirectory, Is.EqualTo($"c:{sep}temp{sep}somefolder"));
            Assert.That(path.RelativePath, Is.EqualTo($"checkout1{sep}src{sep}myproj.csproj"));
            Assert.That(path.FullDirectory, Is.EqualTo($"c:{sep}temp{sep}somefolder{sep}checkout1{sep}src"));
            Assert.That(path.FullPath, Is.EqualTo($"c:{sep}temp{sep}somefolder{sep}checkout1{sep}src{sep}myproj.csproj"));
            Assert.That(path.FileName, Is.EqualTo("myproj.csproj"));
        }
    }
}
