using System;
using System.IO;
using System.Linq;
using System.Text;
using NSubstitute;
using NuGet.Versioning;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
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
        public void UnusualEncodingProjectCanBeRead()
        {
            const string NoProject =
                @"<?xml version=""1.0"" encoding=""Windows-1252""?>
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
            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", "", StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void SinglePackageCanBeRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            Assert.That(packages, Is.Not.Null);
            Assert.That(packages, Is.Not.Empty);
        }

        [Test]
        public void SinglePackageIsPopulated()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            var package = packages.FirstOrDefault();

            PackageAssert.IsPopulated(package);
            Assert.That(package.IsPrerelease, Is.False);
            Assert.That(package.ProjectReferences, Is.Not.Null);
            Assert.That(package.ProjectReferences.Count, Is.EqualTo(1));
        }

        [Test]
        public void ProjectReferencesIsPopulated()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            var package = packages.FirstOrDefault();

            Assert.That(package.ProjectReferences.Count, Is.EqualTo(1));

            StringAssert.EndsWith("other.csproj", package.ProjectReferences.First());
        }

        [Test]
        public void RelativeProjectReferencesIsPopulated()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

            var relativePath = $"..{Path.DirectorySeparatorChar}other{Path.DirectorySeparatorChar}other.csproj";
            projectFile = projectFile.Replace("other.csproj", relativePath, StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile);

            var package = packages.FirstOrDefault();

            Assert.That(package.ProjectReferences.Count, Is.EqualTo(1));

            var path = package.ProjectReferences.First();

            StringAssert.EndsWith($"other{Path.DirectorySeparatorChar}other.csproj", path);
            StringAssert.DoesNotContain("..", path);
        }

        [Test]
        public void SinglePackageIsCorectlyRead()
        {
            const string packagesText = @"<PackageReference Include=""foo"" Version=""1.2.3""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

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

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

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

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3")));

            Assert.That(packages[1].Id, Is.EqualTo("bar"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4")));
        }

        [Test]
        public void WhenTwoPackagesAreRead_ValuesAreCorrect_WithGlobalPackageReference()
        {
            const string packagesText =
                @"<GlobalPackageReference Include=""foo"" Version=""1.2.3""></GlobalPackageReference>
                  <PackageReference Include=""bar"" Version=""2.3.4""></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages[0].Id, Is.EqualTo("bar"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("2.3.4")));

            Assert.That(packages[1].Id, Is.EqualTo("foo"));
            Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("1.2.3")));
        }

        [Test]
        public void WhenThreePackagesAreRead_ValuesAreCorrect_WithImport()
        {
            var temp = Path.GetTempFileName();
            var projectFile = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
  </PropertyGroup>
  <Import Project=""{temp}"" />
</Project>";

            File.WriteAllText(temp, @"<Project><ItemGroup>
<PackageReference Include=""foo"" Version=""1.2.3.4"" />
<PackageReference Update=""bar"" Version=""2.3.4.5"" /></ItemGroup></Project>");
            try
            {
                var reader = MakeReader();
                var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                    .ToList();

                Assert.That(packages[0].Id, Is.EqualTo("foo"));
                Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("1.2.3.4")));
                Assert.That(packages[0].Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));

                Assert.That(packages[1].Id, Is.EqualTo("bar"));
                Assert.That(packages[1].Version, Is.EqualTo(new NuGetVersion("2.3.4.5")));
                Assert.That(packages[1].Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
            }
            finally
            {
                File.Delete(temp);
            }
        }

        [Test]
        public void WhenOnePackagesAreRead_ValuesAreCorrect_WithSdk()
        {
            var projectFile = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
  </PropertyGroup>
  <Sdk Name=""foo"" Version=""4.3.2"" />
</Project>";

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages[0].Id, Is.EqualTo("foo"));
            Assert.That(packages[0].Version, Is.EqualTo(new NuGetVersion("4.3.2")));
            Assert.That(packages[0].Path.PackageReferenceType, Is.EqualTo(PackageReferenceType.DirectoryBuildTargets));
        }

        [Test]
        public void WhenZeroPackagesAreRead_NoVersion_WithSdk()
        {
            var projectFile = @"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp1.1</TargetFramework>
  </PropertyGroup>
  <Sdk Name=""foo"" />
</Project>";

            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages, Is.Empty);
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

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", packagesText, StringComparison.OrdinalIgnoreCase);

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

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", noVersion, StringComparison.OrdinalIgnoreCase);


            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void PackageWithWildCardVersionShouldBeSkipped()
        {
            const string noVersion =
                @"<PackageReference Include=""AWSSDK.Core"" Version=""3.3.*"" />";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", noVersion, StringComparison.OrdinalIgnoreCase);


            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages, Is.Empty);
        }

        [Test]
        public void PackageWithBetaVersionShouldBeRead()
        {
            const string noVersion =
                @"<PackageReference Include=""foo"" Version=""2.0.0-beta01"" />";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", noVersion, StringComparison.OrdinalIgnoreCase);


            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            Assert.That(packages.First().IsPrerelease, Is.True);
        }

        [Test]
        public void PackageWithMetadataShouldBeRead()
        {
            const string noVersion =
                @"<PackageReference Include=""NuGet.Protocol"" Version=""4.7.0+9245481f357ae542f92e6bc5e504fc898cfe5fc0"" />";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", noVersion, StringComparison.OrdinalIgnoreCase);


            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            Assert.That(packages.First().IsPrerelease, Is.False);
        }

        [Test]
        public void PackageWithAssetsVersionShouldBeRead()
        {
            const string noVersion =
                @"<PackageReference Include=""foo""><Version>15.0.26606</Version><ExcludeAssets>all</ExcludeAssets></PackageReference>";

            var projectFile = Vs2017ProjectFileTemplateWithPackages.Replace("{{Packages}}", noVersion, StringComparison.OrdinalIgnoreCase);


            var reader = MakeReader();
            var packages = reader.Read(StreamFromString(projectFile), _sampleDirectory, _sampleFile)
                .ToList();

            Assert.That(packages.Count, Is.EqualTo(1));
            PackageAssert.IsPopulated(packages[0]);
        }

        private static ProjectFileReader MakeReader()
        {
            return new ProjectFileReader(Substitute.For<INuKeeperLogger>());
        }

        private static Stream StreamFromString(string contents)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(contents));
        }
    }
}
