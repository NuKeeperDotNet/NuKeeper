using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Versioning;
using NUnit.Framework;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    [Category("WindowsOnly")] // Windows only due to NuGetUpdatePackageCommand
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
            const string oldPackageVersion = "5.2.3";
            const string newPackageVersion = "5.2.4";
            const string expectedPackageString =
                "<package id=\"Microsoft.AspNet.WebApi.Client\" version=\"{packageVersion}\" targetFramework=\"net47\" />";
            const string testFolder = nameof(ShouldUpdateDotnetClassicProject);

            var testProject = $"{testFolder}.csproj";
            var tempFolder = UniqueTemporaryFolder();

            var workDirectory = Path.Combine(tempFolder.FullPath, testFolder);
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

            var logger = Substitute.For<INuKeeperLogger>();
            var command = new NuGetUpdatePackageCommand(logger, new NuGetPath(logger), new ExternalProcess(logger));

            var packageToUpdate = new PackageInProject("Microsoft.AspNet.WebApi.Client", oldPackageVersion,
                    new PackagePath(workDirectory, testProject, PackageReferenceType.PackagesConfig));

            await command.Invoke(packageToUpdate, new NuGetVersion(newPackageVersion),
                new PackageSource(NuGetConstants.V3FeedUrl), NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(packagesConfigPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion)));

            tempFolder.TryDelete();
        }

        private static IFolder UniqueTemporaryFolder()
        {
            var factory = new FolderFactory(Substitute.For<INuKeeperLogger>());
            return factory.UniqueTemporaryFolder();
        }
    }
}
