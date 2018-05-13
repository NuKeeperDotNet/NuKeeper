using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Integration.Tests.NuGet.Api;
using NuKeeper.Update;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class DotNetUpdatePackageCommandTests
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

        [Test]
        public async Task ShouldNotThrowOnWebProjectMixedStyleUpdates()
        {
            await ExecuteValidUpdateTest(_testWebApiProject);
        }

        [Test]
        public async Task ShouldUpdateDotnetCoreProject()
        {
            await ExecuteValidUpdateTest(_testDotNetCoreProject);
        }

        [Test]
        public async Task ShouldUpdateDuplicateProject()
        {
            const string name = nameof(ShouldUpdateDuplicateProject);
            var projectPath = Path.Combine(TestContext.CurrentContext.WorkDirectory, name, $"AnotherProject.csproj");
            Directory.CreateDirectory(Path.GetDirectoryName(projectPath));
            using (File.Create(projectPath))
            {
                // close file stream automatically
            };

            await ExecuteValidUpdateTest(_testDotNetCoreProject);
        }

        [Test]
        public async Task ShouldUpdateDotnetClassicWithPackageReference()
        {
            await ExecuteValidUpdateTest(_testDotNetClassicProject);
        }

        private async Task ExecuteValidUpdateTest(string testProjectContents, [CallerMemberName] string memberName = "")
        {
            const string packageSource = "https://api.nuget.org/v3/index.json";
            const string oldPackageVersion = "5.2.3";
            const string newPackageVersion = "5.2.4";
            const string expectedPackageString =
                "<PackageReference Include=\"Microsoft.AspNet.WebApi.Client\" Version=\"{packageVersion}\" />";

            var testFolder = memberName;
            var testProject = $"{memberName}.csproj";
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, testFolder);
            Directory.CreateDirectory(workDirectory);
            var projectContents = testProjectContents.Replace("{packageVersion}", oldPackageVersion);
            var projectPath = Path.Combine(workDirectory, testProject);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var command = new DotNetUpdatePackageCommand(
                    new NullNuKeeperLogger(),
                    new NuGetSources(packageSource));

            await command.Invoke(new NuGetVersion(newPackageVersion), packageSource,
                new PackageInProject("Microsoft.AspNet.WebApi.Client", oldPackageVersion,
                    new PackagePath(workDirectory, testProject, PackageReferenceType.ProjectFile)));

            var contents = await File.ReadAllTextAsync(projectPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion)));
        }
    }
}
