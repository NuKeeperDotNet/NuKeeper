using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NuGet.Versioning;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class UpdateDirectoryBuildTargetsCommandPackageDownloadTests : TestWithFailureLogging
    {
        private readonly string _testFileWithUpdate =
            @"<Project><ItemGroup><PackageDownload Update=""foo"" Version=""[{packageVersion}]"" /></ItemGroup></Project>";

        private readonly string _testFileWithInclude =
            @"<Project><ItemGroup><PackageDownload Include=""foo"" Version=""[{packageVersion}]"" /></ItemGroup></Project>";

        [Test]
        public async Task ShouldUpdateValidFileWithUpdateAttribute()
        {
            await ExecuteValidUpdateTest(_testFileWithUpdate, "<PackageDownload Update=\"foo\" Version=\"[{packageVersion}]\" />");
        }

        [Test]
        public async Task ShouldUpdateValidFileWithIncludeAttribute()
        {
            await ExecuteValidUpdateTest(_testFileWithInclude, "<PackageDownload Include=\"foo\" Version=\"[{packageVersion}]\" />");
        }

        [Test]
        public async Task ShouldUpdateValidFileWithIncludeAndVerboseVersion()
        {
            await ExecuteValidUpdateTest(
                @"<Project><ItemGroup><PackageDownload Include=""foo""><Version>[{packageVersion}]</Version></PackageDownload></ItemGroup></Project>",
                @"<Version>[{packageVersion}]</Version>");
        }

        private async Task ExecuteValidUpdateTest(string testProjectContents, string expectedPackageString, [CallerMemberName] string memberName = "")
        {
            const string oldPackageVersion = "5.2.31";
            const string newPackageVersion = "5.3.4";

            var testFolder = memberName;
            var testFile = "Directory.Build.props";
            var workDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, testFolder);
            Directory.CreateDirectory(workDirectory);
            var projectContents = testProjectContents.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase);
            var projectPath = Path.Combine(workDirectory, testFile);
            await File.WriteAllTextAsync(projectPath, projectContents);

            var command = new UpdateDirectoryBuildTargetsCommand(NukeeperLogger);

            var package = new PackageInProject("foo", oldPackageVersion,
                new PackagePath(workDirectory, testFile, PackageReferenceType.DirectoryBuildTargets));

            await command.Invoke(package, new NuGetVersion(newPackageVersion), null, NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(projectPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion, StringComparison.OrdinalIgnoreCase)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase)));
        }
    }
}