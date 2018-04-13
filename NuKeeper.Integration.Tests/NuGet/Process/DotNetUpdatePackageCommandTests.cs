using System.IO;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.NuGet.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class DotNetUpdatePackageCommandTests
    {
        private readonly string _testWebApiProject =
@"<Project ToolsVersion=""15.0"" DefaultTargets=""Build"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <Import Project=""$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props"" Condition=""Exists('$(MSBuildExtensionsPath)\$(MSBuildToolsVersion)\Microsoft.Common.props')"" />
  <ItemGroup><PackageReference Include=""Microsoft.AspNet.WebApi.Client""><Version>5.2.3</Version></PackageReference></ItemGroup>
  <PropertyGroup>
    <VisualStudioVersion Condition=""'$(VisualStudioVersion)' == ''"">10.0</VisualStudioVersion>
    <VSToolsPath Condition=""'$(VSToolsPath)' == ''"">$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)</VSToolsPath>
    <TargetFrameworkVersion>v4.7</TargetFrameworkVersion>
  </PropertyGroup>
  <Import Project=""$(MSBuildBinPath)\Microsoft.CSharp.targets"" />
  <Import Project=""$(VSToolsPath)\WebApplications\Microsoft.WebApplication.targets"" Condition=""'$(VSToolsPath)' != ''"" />
</Project>";

        [Test]
        [Ignore("Known failure, issue #239")]
        public async Task ShouldNotThrowOnWebProjectMixedStyleUpdates()
        {
            const string testFolder = nameof(ShouldNotThrowOnWebProjectMixedStyleUpdates);
            const string testProject = "TestWebApiProject.csproj";
            const string packageSource = "https://api.nuget.org/v3/index.json";
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, testFolder);
            Directory.CreateDirectory(workDirectory);
            File.WriteAllText(Path.Combine(workDirectory, testProject), _testWebApiProject);

            var command =
                new DotNetUpdatePackageCommand(
                    new UserSettings {NuGetSources = new[] {packageSource}});

            await command.Invoke(new NuGetVersion("5.2.4"), packageSource,
                new PackageInProject("Microsoft.AspNet.WebApi.Client", "5.2.3",
                    new PackagePath(workDirectory, testProject, PackageReferenceType.ProjectFile)));
        }
    }
}
