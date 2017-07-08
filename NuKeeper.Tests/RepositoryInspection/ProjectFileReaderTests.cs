using System.Linq;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class ProjectFileReaderTests
    {
        private const string Vs2017ProjectFileTemplateWithPackages =
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
{{Packages}}
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include=""other.csproj"" />
  </ItemGroup>
</Project>";

        private const string Vs2017ProjectFileTemplateWithoutPackages =
            @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""other.csproj"" />
  </ItemGroup>
</Project>";

        [Test]
        public void NoProjectCanBeRead()
        {
            const string NoProject =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<foo>
</foo>";

            var packages = ProjectFileReader.Read(NoProject);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void EmptyProjectCanBeRead()
        {
            const string NoProject =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project>
</Project>";

            var packages = ProjectFileReader.Read(NoProject);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void ProjectWithoutPackageListCanBeRead()
        {
            var packages = ProjectFileReader.Read(Vs2017ProjectFileTemplateWithoutPackages);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void ProjectWithEmptyPackageListCanBeRead()
        {
            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", "");

            var packages = ProjectFileReader.Read(projectFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void SinglePackageCanBeRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var packages = ProjectFileReader.Read(projectFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageIsCorectlyRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var packages = ProjectFileReader.Read(projectFile);

            var package = packages.FirstOrDefault();

            Assert.That(package, Is.Not.Null);
            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3")));
        }

        [Test]
        public void TwoPackagesCanBeRead()
        {
            const string packagesText =
                @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var packages = ProjectFileReader.Read(projectFile)
                .ToList();

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages.Count, Is.EqualTo(2));
            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[1].Id, Is.EqualTo("bar"));
        }

        [Test]
        public void ResultIsReiterable()
        {
            const string marker = "marker";

            var packages = ProjectFileReader.Read(Vs2017ProjectFileTemplateWithPackages);

            foreach (var package in packages)
            {
                package.SourceFilePath = marker;
            }

            Assert.That(packages.Select(p=>p.SourceFilePath), Is.All.EqualTo(marker));
        }
    }
}
