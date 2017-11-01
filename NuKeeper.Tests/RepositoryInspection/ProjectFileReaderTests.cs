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

        private const string Vs2017ProjectFileFullFrameworkWithPackages =
            @"<?xml version=""1.0"" encoding=""utf-8""?>
<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"" ToolsVersion=""14.0"" DefaultTargets=""Build"">
  <ItemGroup>
    <PackageReference Include=""StyleCop.Analyzers"">
      <Version>1.0.2</Version>
    </PackageReference>
  </ItemGroup>
</Project>
";

        [Test]
        public void NoProjectCanBeRead()
        {
            const string NoProject =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<foo>
</foo>";

            var reader = MakeReader();
            var packages = reader.Read(NoProject, TempPath());

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

            var reader = MakeReader();
            var packages = reader.Read(NoProject, TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void ProjectWithoutPackageListCanBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(Vs2017ProjectFileTemplateWithoutPackages, TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void ProjectWithEmptyPackageListCanBeRead()
        {
            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", "");

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void SinglePackageCanBeRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath());

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageIsPopulated()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath());

            var package = packages.FirstOrDefault();

            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SinglePackageIsCorectlyRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath());

            var package = packages.FirstOrDefault();

            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void SinglePackageFullFrameworkProjectIsCorectlyRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(Vs2017ProjectFileFullFrameworkWithPackages, TempPath());

            var package = packages.Single();

            Assert.That(package.Id, Is.EqualTo("StyleCop.Analyzers"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.0.2")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void WhenTwoPackagesAreRead_TheyArePopulated()
        {
            const string packagesText =
                @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath())
                .ToList();

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages.Count, Is.EqualTo(2));
            PackageAssert.IsPopulated(packages[0]);
            PackageAssert.IsPopulated(packages[1]);
        }

        [Test]
        public void WhenTwoPackagesAreRead_ValuesAreCorrect()
        {
            const string packagesText =
                @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath())
                .ToList();

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4")));
        }

        [Test]
        public void ResultIsReiterable()
        {
            var path = TempPath();

            var reader = MakeReader();
            var packages = reader.Read(Vs2017ProjectFileTemplateWithPackages, path);

            foreach (var package in packages)
            {
                Assert.That(package, Is.Not.Null);
            }

            Assert.That(packages.Select(p=>p.Path), Is.All.EqualTo(path));
        }

        [Test]
        public void WhenOnePackageCannotBeRead_TheOthersAreStillRead()
        {
            const string packagesText =
                @"<PackageReference Include=""foo"" Version=""notaversion""></PackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(projectFile, TempPath())
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            PackageAssert.IsPopulated(packages[0]);
        }

        private PackagePath TempPath()
        {
            return new PackagePath("c:\\temp\\somewhere", "src\\packages.config",
                PackageReferenceType.ProjectFile);
        }

        private ProjectFileReader MakeReader()
        {
            return new ProjectFileReader(new NullNuKeeperLogger());
        }
    }
}
