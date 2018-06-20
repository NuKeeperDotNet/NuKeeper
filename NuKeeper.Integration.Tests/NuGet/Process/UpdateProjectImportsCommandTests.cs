using System.IO;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class UpdateProjectImportsCommandTests
    {
        private readonly string _testWebApiProject =
            @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <Import Project=""$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets"" Condition=""'$(VSToolsPath)' != ''"" />
  <Import Project=""$(VSToolsPath)\DummyImportWithoutCondition\Microsoft.WebApplication.targets"" />
</Project>";

        private readonly string _projectWithReference =
            @"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003""><ItemGroup><ProjectReference Include=""{importPath}"" /></ItemGroup></Project>";

        private readonly string _unpatchedImport =
            @"<Import Project=""$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets"" Condition=""'$(VSToolsPath)' != ''"" />";

        private readonly string _patchedImport =
            @"<Import Project=""$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets"" Condition=""Exists('$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets')"" />";

        [Test]
        public async Task ShouldUpdateConditionOnTaskImport()
        {
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                nameof(ShouldUpdateConditionOnTaskImport));

            Directory.CreateDirectory(workDirectory);
            var projectName = nameof(ShouldUpdateConditionOnTaskImport) + ".csproj";
            var projectPath = Path.Combine(workDirectory, projectName);
            await File.WriteAllTextAsync(projectPath, _testWebApiProject);

            var subject = new UpdateProjectImportsCommand();

            var package = new PackageInProject("acme", "1",
                new PackagePath(workDirectory, projectName, PackageReferenceType.ProjectFileOldStyle));

            await subject.Invoke(package, null, null, NuGetSources.GlobalFeed);

            var updatedContents = await File.ReadAllTextAsync(projectPath);

            Assert.That(updatedContents, Does.Not.Contain(_unpatchedImport));
            Assert.That(updatedContents, Does.Contain(_patchedImport));
        }

        [Test]
        public async Task ShouldFollowResolvableImports()
        {
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory,
                nameof(ShouldFollowResolvableImports));

            Directory.CreateDirectory(workDirectory);

            var projectName = nameof(ShouldFollowResolvableImports) + ".csproj";
            var projectPath = Path.Combine(workDirectory, projectName);
            await File.WriteAllTextAsync(projectPath, _testWebApiProject);

            var intermediateProject = Path.Combine(workDirectory, "Intermediate.csproj");
            var intermediateContents = _projectWithReference.Replace("{importPath}", projectPath);
            await File.WriteAllTextAsync(intermediateProject, intermediateContents);

            var rootProject = Path.Combine(workDirectory, "RootProject.csproj");
            var rootContets = _projectWithReference.Replace("{importPath}",
                Path.Combine("..", nameof(ShouldFollowResolvableImports), "Intermediate.csproj"));
            await File.WriteAllTextAsync(rootProject, rootContets);

            var subject = new UpdateProjectImportsCommand();

            var package = new PackageInProject("acme", "1",
                new PackagePath(workDirectory, "RootProject.csproj", PackageReferenceType.ProjectFileOldStyle));

            await subject.Invoke(package, null, null, NuGetSources.GlobalFeed);

            var updatedContents = await File.ReadAllTextAsync(projectPath);

            Assert.That(updatedContents, Does.Not.Contain(_unpatchedImport));
            Assert.That(updatedContents, Does.Contain(_patchedImport));
        }
    }
}
