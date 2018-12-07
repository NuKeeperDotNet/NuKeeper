using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Versioning;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Integration.Tests.NuGet.Process
{
    [TestFixture]
    public class UpdateDirectoryBuildTargetsCommandTests
    {
        private readonly string _testFileWithUpdate =
@"<Project><ItemGroup><PackageReference Update=""foo"" Version=""{packageVersion}"" /></ItemGroup></Project>";

        private readonly string _testFileWithInclude =
@"<Project><ItemGroup><PackageReference Include=""foo"" Version=""{packageVersion}"" /></ItemGroup></Project>";

        [Test]
        public async Task ShouldUpdateValidFileWithUpdateAttribute()
        {
            await ExecuteValidUpdateTest(_testFileWithUpdate, "<PackageReference Update=\"foo\" Version=\"{packageVersion}\" />");
        }

        [Test]
        public async Task ShouldUpdateValidFileWithIncludeAttribute()
        {
            await ExecuteValidUpdateTest(_testFileWithInclude, "<PackageReference Include=\"foo\" Version=\"{packageVersion}\" />");
        }

        private static async Task ExecuteValidUpdateTest(string testProjectContents, string expectedPackageString, [CallerMemberName] string memberName = "")
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

            var command = new UpdateDirectoryBuildTargetsCommand(Substitute.For<INuKeeperLogger>());

            var package = new PackageInProject("foo", oldPackageVersion,
                new PackagePath(workDirectory, testFile, PackageReferenceType.DirectoryBuildTargets));

            await command.Invoke(package, new NuGetVersion(newPackageVersion), null, NuGetSources.GlobalFeed);

            var contents = await File.ReadAllTextAsync(projectPath);
            Assert.That(contents, Does.Contain(expectedPackageString.Replace("{packageVersion}", newPackageVersion, StringComparison.OrdinalIgnoreCase)));
            Assert.That(contents, Does.Not.Contain(expectedPackageString.Replace("{packageVersion}", oldPackageVersion, StringComparison.OrdinalIgnoreCase)));
        }
    }
}
