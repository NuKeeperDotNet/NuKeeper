using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Integration.Tests.NuGet.Api;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class UpdateNuspecCommandTests
    {
        private readonly string _testNuspec =
@"<package><metadata><dependencies>
      <dependency id=""foo"" version=""{packageVersion}"" />
</dependencies></metadata></package>
";

        [Test]
        public async Task ShouldUpdateValidNuspecFile()
        {
            await ExecuteValidUpdateTest(_testNuspec);
        }

        private async Task ExecuteValidUpdateTest(string testProjectContents, [CallerMemberName] string memberName = "")
        {
            const string oldPackageVersion = "5.2.3";
            const string newPackageVersion = "5.2.4";
            const string expectedPackageString =
                "<dependency id=\"foo\" version=\"{packageVersion}\" />";

            var testFolder = memberName;
            var testNuspec = $"{memberName}.nuspec";
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, testFolder);
            Directory.CreateDirectory(workDirectory);
            var projectContents = testProjectContents.Replace("{packageVersion}", oldPackageVersion);
            var projectPath = Path.Combine(workDirectory, testNuspec);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var command = new UpdateNuspecCommand(new NullNuKeeperLogger());

            var package = new PackageInProject("foo", oldPackageVersion,
                new PackagePath(workDirectory, testNuspec, PackageReferenceType.Nuspec));

            await command.Invoke(package, new NuGetVersion(newPackageVersion), null, NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(projectPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion)));
        }
    }
}
