using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class DotNetUpdatePackageCommandTests : TestWithFailureLogging
    {
        private readonly string _testWebApiProject =
@"<Project ToolsVersion=""15.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <ItemGroup><PackageReference Include=""Microsoft.AspNet.WebApi.Client""><Version>{packageVersion}</Version></PackageReference></ItemGroup>
  <PropertyGroup>
    <VisualStudioVersion Condition=""'$(VisualStudioVersion)' == ''"">10.0</VisualStudioVersion>
    <VSToolsPath Condition=""'$(VSToolsPath)' == ''"">$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
    <TargetFrameworkVersion>v4.7</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <!-- without the second package, 'dotnet add' will refuse to run -->
    <!-- as the heuristic will consider this a packages.config project -->
    <PackageReference Include=""Newtonsoft.Json"" Version=""11.0.2"" />
  </ItemGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <Import Project=""$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets"" Condition=""Exists('$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets')"" />
</Project>";

        private readonly string _testDotNetCoreProject =
@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.0</TargetFramework>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNet.WebApi.Client"" Version=""{packageVersion}"" />
  </ItemGroup>
</Project>
";

        private readonly string _testDotNetClassicProject =
@"<Project ToolsVersion=""14.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <TargetFrameworkVersion>v4.6.1</TargetFrameworkVersion>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""Microsoft.AspNet.WebApi.Client"" Version=""{packageVersion}"" />
    <!-- without the second package, 'dotnet add' will refuse to run -->
    <!-- as the heuristic will consider this a packages.config project -->
    <PackageReference Include=""Newtonsoft.Json"">
      <Version>11.0.2</Version>
    </PackageReference>
  </ItemGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>";

        private IFolder _uniqueTemporaryFolder;

        [SetUp]
        public void Setup()
        {
            _uniqueTemporaryFolder = UniqueTemporaryFolder();
        }

        [TearDown]
        public void TearDown()
        {
            _uniqueTemporaryFolder.TryDelete();
        }

        [Test]
        public async Task ShouldNotThrowOnWebProjectMixedStyleUpdates()
        {
            await ExecuteValidUpdateTest(_testWebApiProject, PackageReferenceType.ProjectFileOldStyle);
        }

        [Test]
        public async Task ShouldUpdateDotnetCoreProject()
        {
            await ExecuteValidUpdateTest(_testDotNetCoreProject, PackageReferenceType.ProjectFile);
        }

        [Test]
        public async Task ShouldUpdateDuplicateProject()
        {
            const string name = nameof(ShouldUpdateDuplicateProject);
            var projectPath = Path.Combine(_uniqueTemporaryFolder.FullPath, name, "AnotherProject.csproj");
            Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
            using (File.Create(projectPath))
            {
                // close file stream automatically
            }

            await ExecuteValidUpdateTest(_testDotNetCoreProject, PackageReferenceType.ProjectFile);
        }

        [Test]
        public async Task ShouldUpdateDotnetClassicWithPackageReference()
        {
            await ExecuteValidUpdateTest(_testDotNetClassicProject, PackageReferenceType.ProjectFileOldStyle);
        }

        [Test]
        public async Task ShouldUpdateProjectFilenameWithSpaces()
        {
            await ExecuteValidUpdateTest(_testDotNetClassicProject, PackageReferenceType.ProjectFileOldStyle, "Project With Spaces.csproj");
        }


        private async Task ExecuteValidUpdateTest(
            string testProjectContents,
            PackageReferenceType packageReferenceType,
            [CallerMemberName] string memberName = "")
        {
            const string oldPackageVersion = "5.2.3";
            const string newPackageVersion = "5.2.4";
            const string expectedPackageString =
                "<PackageReference Include=\"Microsoft.AspNet.WebApi.Client\" Version=\"{packageVersion}\" />";

            var testFolder = memberName;
            var testProject = $"{memberName}.csproj";

            var workDirectory = Path.Combine(_uniqueTemporaryFolder.FullPath, testFolder);
            Directory.CreateDirectory(workDirectory);

            var projectContents = testProjectContents.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase);
            var projectPath = Path.Combine(workDirectory, testProject);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var command = new DotNetUpdatePackageCommand(new ExternalProcess(NukeeperLogger));

            var packageToUpdate = new PackageInProject("Microsoft.AspNet.WebApi.Client", oldPackageVersion,
                new PackagePath(workDirectory, testProject, packageReferenceType));

            await command.Invoke(packageToUpdate, new NuGetVersion(newPackageVersion),
                new PackageSource(NuGetConstants.V3FeedUrl), NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(projectPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion, StringComparison.OrdinalIgnoreCase)));
            Assert.That(contents,
                Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase)));
        }

        private IFolder UniqueTemporaryFolder()
        {
            var factory = new FolderFactory(NukeeperLogger);
            return factory.UniqueTemporaryFolder();
        }
    }
}
