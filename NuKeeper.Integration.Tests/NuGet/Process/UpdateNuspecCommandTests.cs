using NuGet.Versioning;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update.Process;
using NUnit.Framework;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class UpdateNuspecCommandTests : BaseTest
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
            const string oldPackageVersion = "5.2.31";
            const string newPackageVersion = "5.3.4";
            const string expectedPackageString =
                "<dependency id=\"foo\" version=\"{packageVersion}\" />";

            var testFolder = memberName;
            var testNuspec = $"{memberName}.nuspec";
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, testFolder);
            Directory.CreateDirectory(workDirectory);
            var projectContents = testProjectContents.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase);
            var projectPath = Path.Combine(workDirectory, testNuspec);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var command = new UpdateNuspecCommand(NukeeperLogger);

            var package = new PackageInProject("foo", oldPackageVersion,
                new PackagePath(workDirectory, testNuspec, PackageReferenceType.Nuspec));

            await command.Invoke(package, new NuGetVersion(newPackageVersion), null, NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(projectPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion, StringComparison.OrdinalIgnoreCase)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
