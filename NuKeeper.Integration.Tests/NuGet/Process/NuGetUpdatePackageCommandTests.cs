using System.IO;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Integration.Tests.NuGet.Api;
using NuKeeper.NuGet.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    [Category("WindowsOnly")]
    public class NuGetUpdatePackageCommandTests
    {
        private readonly string _testDotNetClassicProject =
@"<Project ToolsVersion=""15.0"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <PropertyGroup>
    <TargetFrameworkVersion>v4.7</TargetFrameworkVersion>
  </PropertyGroup>
  <Import Project=""$(MSBuildToolsPath)\Microsoft.CSharp.targets"" />
</Project>";

        private readonly string _testPackagesConfig =
@"<packages><package id=""Microsoft.AspNet.WebApi.Client"" version=""{packageVersion}"" targetFramework=""net47"" /></packages>";

        private readonly string _nugetConfig =
@"<configuration><config><add key=""repositoryPath"" value="".\packages"" /></config></configuration>";

        [Test]
        public async Task ShouldUpdateDotnetClassicProject()
        {
            const string packageSource = "https://api.nuget.org/v3/index.json";
            const string oldPackageVersion = "5.2.3";
            const string newPackageVersion = "5.2.4";
            const string expectedPackageString =
                "<package id=\"Microsoft.AspNet.WebApi.Client\" version=\"{packageVersion}\" targetFramework=\"net47\" />";
            const string testFolder = nameof(ShouldUpdateDotnetClassicProject);

            var testProject = $"{testFolder}.csproj";
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, testFolder);
            Directory.CreateDirectory(workDirectory);
            var packagesFolder = Path.Combine(workDirectory, "packages");
            Directory.CreateDirectory(packagesFolder);

            var projectContents = _testDotNetClassicProject.Replace("{packageVersion}", oldPackageVersion);
            var projectPath = Path.Combine(workDirectory, testProject);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var packagesConfigContents = _testPackagesConfig.Replace("{packageVersion}", oldPackageVersion);
            var packagesConfigPath = Path.Combine(workDirectory, "packages.config");
            await File.WriteAllTextAsync(packagesConfigPath, packagesConfigContents);

            await File.WriteAllTextAsync(Path.Combine(workDirectory, "nuget.config"), _nugetConfig);

            var command =
                new NuGetUpdatePackageCommand(
                    new NullNuKeeperLogger(),
                    new UserSettings { NuGetSources = new[] { packageSource } });

            await command.Invoke(new NuGetVersion(newPackageVersion), packageSource,
                new PackageInProject("Microsoft.AspNet.WebApi.Client", oldPackageVersion,
                    new PackagePath(workDirectory, testProject, PackageReferenceType.PackagesConfig)));

            var contents = await File.ReadAllTextAsync(packagesConfigPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion)));
        }
    }
}
