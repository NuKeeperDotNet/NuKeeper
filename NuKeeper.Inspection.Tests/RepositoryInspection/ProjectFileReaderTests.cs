using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Versioning;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.RepositoryInspection
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

        private string _sampleDirectory;
        private string _sampleFile;

        [SetUp]
        public void SetUp()
        {
            _sampleDirectory = OsSpecifics.GenerateBaseDirectory();
            _sampleFile = Path.Combine("src", "packages.config");
        }

        [Test]
        public void NoProjectCanBeRead()
        {
            const string NoProject =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
<foo>
</foo>";

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(NoProject), _sampleDirectory, _sampleFile);

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
            var packages = reader.Read(StreamFromString(NoProject), _sampleDirectory, _sampleFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void ProjectWithoutPackageListCanBeRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(Vs2017ProjectFileTemplateWithoutPackages), _sampleDirectory, _sampleFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void ProjectWithEmptyPackageListCanBeRead()
        {
            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", "");

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void SinglePackageCanBeRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageIsPopulated()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            var package = packages.FirstOrDefault();

            PackageAssert.IsPopulated(package);
        }

        [Test]
        public void SinglePackageIsCorectlyRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            var package = packages.FirstOrDefault();

            Assert.That(package.Id, Is.EqualTo("foo"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.2.3")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFile));
        }

        [Test]
        public void SinglePackageFullFrameworkProjectIsCorectlyRead()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(Vs2017ProjectFileFullFrameworkWithPackages), _sampleDirectory, _sampleFile);

            var package = packages.Single();

            Assert.That(package.Id, Is.EqualTo("StyleCop.Analyzers"));
            Assert.That(package.Version, Is.EqualTo(new NuGetVersion("1.0.2")));
            Assert.That(package.Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.ProjectFileOldStyle));
        }

        [Test]
        public void WhenTwoPackagesAreRead_TheyArePopulated()
        {
            const string packagesText =
                @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
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
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4")));
        }

        [Test]
        public void ResultIsReiterable()
        {
            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(Vs2017ProjectFileTemplateWithPackages), _sampleDirectory, _sampleFile);

            foreach (var package in packages)
            {
                Assert.That(package, Is.Not.Null);
            }

            Assert.That(packages.Select(p => p.Path),
                Is.All.EqualTo(new PackagePath(_sampleDirectory, _sampleFile, PackageReferenceType.ProjectFile)));
        }

        [Test]
        public void WhenOnePackageCannotBeRead_TheOthersAreStillRead()
        {
            const string packagesText =
                @"<PackageReference Include=""foo"" Version=""notaversion""></PackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            PackageAssert.IsPopulated(packages[0]);
        }

        [Test]
        public void PackageWithoutVersionShouldBeSkipped()
        {
            const string noVersion =
                @"<PackageReference Include=""Microsoft.AspNetCore.App"" />";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", noVersion);


            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages, Is.Empty);
        }

        private ProjectFileReader MakeReader()
        {
            return new ProjectFileReader(Substitute.For<INuKeeperLogger>());
        }

        private Stream StreamFromString(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
