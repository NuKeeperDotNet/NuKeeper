using System;
using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Versioning;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NUnit.Framework;
using NuKeeper.Inspection.Files;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using NuKeeper.Abstractions.RepositoryInspection;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class NuGetUpdatePackageCommandTests : BaseTest
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

        private IFolder _uniqueTemporaryFolder = null;

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
        public async Task ShouldUpdateDotnetClassicProject()
        {
            const string oldPackageVersion = "5.2.3";
            const string newPackageVersion = "5.2.4";
            const string expectedPackageString =
                "<package id=\"Microsoft.AspNet.WebApi.Client\" version=\"{packageVersion}\" targetFramework=\"net47\" />";
            const string testFolder = nameof(ShouldUpdateDotnetClassicProject);

            var testProject = $"{testFolder}.csproj";

            var workDirectory = Path.Combine(_uniqueTemporaryFolder.FullPath, testFolder);
            Directory.CreateDirectory(workDirectory);
            var packagesFolder = Path.Combine(workDirectory, "packages");
            Directory.CreateDirectory(packagesFolder);

            var projectContents = _testDotNetClassicProject.Replace("{packageVersion}", oldPackageVersion,
                StringComparison.OrdinalIgnoreCase);
            var projectPath = Path.Combine(workDirectory, testProject);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var packagesConfigContents = _testPackagesConfig.Replace("{packageVersion}", oldPackageVersion,
                StringComparison.OrdinalIgnoreCase);
            var packagesConfigPath = Path.Combine(workDirectory, "packages.config");
            await File.WriteAllTextAsync(packagesConfigPath, packagesConfigContents);

            await File.WriteAllTextAsync(Path.Combine(workDirectory, "nuget.config"), _nugetConfig);

            var logger = NukeeperLogger;
            var externalProcess = new ExternalProcess(logger);

            var monoExecutor = new MonoExecutor(logger, externalProcess);

            var nuGetPath = new NuGetPath(logger);
            var nuGetVersion = new NuGetVersion(newPackageVersion);
            var packageSource = new PackageSource(NuGetConstants.V3FeedUrl);

            var restoreCommand = new NuGetFileRestoreCommand(logger, nuGetPath, monoExecutor, externalProcess);
            var updateCommand = new NuGetUpdatePackageCommand(logger, nuGetPath, monoExecutor, externalProcess);

            var packageToUpdate = new PackageInProject("Microsoft.AspNet.WebApi.Client", oldPackageVersion,
                new PackagePath(workDirectory, testProject, PackageReferenceType.PackagesConfig));

            await restoreCommand.Invoke(packageToUpdate, nuGetVersion, packageSource, NuGetSources.GlobalFeed);

            await updateCommand.Invoke(packageToUpdate, nuGetVersion, packageSource, NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(packagesConfigPath);
            Assert.That(contents,
                Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion,
                    StringComparison.OrdinalIgnoreCase)));
            Assert.That(contents,
                Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion,
                    StringComparison.OrdinalIgnoreCase)));
        }

        private IFolder UniqueTemporaryFolder()
        {
            var factory = new FolderFactory(NukeeperLogger);
            return factory.UniqueTemporaryFolder();
        }
    }
}
