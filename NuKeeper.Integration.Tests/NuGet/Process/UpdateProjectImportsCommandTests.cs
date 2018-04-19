using System.IO;
using System.Threading.Tasks;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.NuGet.Process;
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
            var projectName = "WebApiProject.csproj";
            var projectPath = Path.Combine(workDirectory, projectName);
            await File.WriteAllTextAsync(projectPath, _testWebApiProject);

            var subject = new UpdateProjectImportsCommand();

            await subject.Invoke(null, null,
                new PackageInProject("acme", "1",
                    new PackagePath(workDirectory, projectName, PackageReferenceType.ProjectFileOldStyle)));

            var updatedContents = await File.ReadAllTextAsync(projectPath);

            Assert.That(updatedContents, Does.Not.Contain(_unpatchedImport));
            Assert.That(updatedContents, Does.Contain(_patchedImport));
        }
    }
}
