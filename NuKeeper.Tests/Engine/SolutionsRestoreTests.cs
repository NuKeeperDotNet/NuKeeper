using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.NuGetApi;
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
            var packages = new List<PackageUpdateSet>()
            {
                UpdateSet(PackageReferenceType.ProjectFile),
            };

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
            var packages = new List<PackageUpdateSet>
            {
                UpdateSet(PackageReferenceType.PackagesConfig)
            };

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
            var packages = new List<PackageUpdateSet>
            {
                UpdateSet(PackageReferenceType.PackagesConfig)
            };

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

        private static PackageUpdateSet UpdateSet(PackageReferenceType refType)
        {
            var fooPackage = new PackageIdentity("foo", new NuGetVersion(1, 2, 3));
            var path = new PackagePath("c:\\foo", "bar", refType);
            var packages = new[]
            {
                new PackageInProject(fooPackage, path, null)
            };

            var latest = new PackageSearchMedatadata(fooPackage, new PackageSource(NuGetConstants.V3FeedUrl), null, null);

            var updates = new PackageLookupResult(VersionChange.Major, latest, null, null, null);
            return new PackageUpdateSet(updates, packages);
        }

    }
}
