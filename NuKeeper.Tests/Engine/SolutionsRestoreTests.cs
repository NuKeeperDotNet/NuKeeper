using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Update.Process;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class SolutionsRestoreTests
    {
        [Test]
        public async Task WhenThereAreNoSolutionsTheCommandIsNotCalled()
        {
            var cmd = Substitute.For<IFileRestoreCommand>();
            var folder = Substitute.For<IFolder>();

            var packages = new List<PackageUpdateSet>();

            var solutionRestore = new SolutionsRestore(cmd);

            await solutionRestore.CheckRestore(packages, folder, NuGetSources.GlobalFeed);

            await cmd.DidNotReceiveWithAnyArgs()
                .Invoke(Arg.Any<FileInfo>(), Arg.Any<NuGetSources>());
        }

        [Test]
        public async Task WhenThereAreNoMatchingPackagesTheCommandIsNotCalled()
        {
            var packages = PackageUpdates.ForPackageRefType(PackageReferenceType.ProjectFile)
                .InList();

            var sln = new FileInfo("foo.sln");

            var cmd = Substitute.For<IFileRestoreCommand>();
            var folder = Substitute.For<IFolder>();
            folder.Find(Arg.Any<string>()).Returns(new[] { sln });

            var solutionRestore = new SolutionsRestore(cmd);

            await solutionRestore.CheckRestore(packages, folder, NuGetSources.GlobalFeed);

            await cmd.DidNotReceiveWithAnyArgs()
                .Invoke(Arg.Any<FileInfo>(), Arg.Any<NuGetSources>());
        }

        [Test]
        public async Task WhenThereIsOneSolutionsTheCommandIsCalled()
        {
            var packages = PackageUpdates.ForPackageRefType(PackageReferenceType.PackagesConfig)
                .InList();

            var sln = new FileInfo("foo.sln");

            var cmd = Substitute.For<IFileRestoreCommand>();
            var folder = Substitute.For<IFolder>();
            folder.Find(Arg.Any<string>()).Returns(new[] { sln });

            var solutionRestore = new SolutionsRestore(cmd);

            await solutionRestore.CheckRestore(packages, folder, NuGetSources.GlobalFeed);

            await cmd.Received(1).Invoke(Arg.Any<FileInfo>(), Arg.Any<NuGetSources>());
            await cmd.Received().Invoke(sln, Arg.Any<NuGetSources>());
        }

        [Test]
        public async Task WhenThereAreTwoSolutionsTheCommandIsCalledForEachOfThem()
        {
            var packages = PackageUpdates.ForPackageRefType(PackageReferenceType.PackagesConfig)
                .InList();

            var sln1 = new FileInfo("foo.sln");
            var sln2 = new FileInfo("bar.sln");

            var cmd = Substitute.For<IFileRestoreCommand>();
            var folder = Substitute.For<IFolder>();
            folder.Find(Arg.Any<string>()).Returns(new[] { sln1, sln2 });

            var solutionRestore = new SolutionsRestore(cmd);

            await solutionRestore.CheckRestore(packages, folder, NuGetSources.GlobalFeed);

            await cmd.Received(2).Invoke(Arg.Any<FileInfo>(), Arg.Any<NuGetSources>());
            await cmd.Received().Invoke(sln1, Arg.Any<NuGetSources>());
            await cmd.Received().Invoke(sln2, Arg.Any<NuGetSources>());
        }
    }
}
