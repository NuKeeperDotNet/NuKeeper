using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Local;
using NuKeeper.Update;
using NuKeeper.Update.Process;
using NuKeeper.Update.Selection;
using NUnit.Framework;

namespace NuKeeper.Tests.Local
{
    [TestFixture]
    public class LocalUpdaterTests
    {
        [Test]
        public async Task EmptyListCase()
        {
            var selection = Substitute.For<IUpdateSelection>();
            var runner = Substitute.For<IUpdateRunner>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();
            var restorer = new SolutionsRestore(Substitute.For<IFileRestoreCommand>());

            var updater = new LocalUpdater(selection, runner, restorer, logger);

            await updater.ApplyUpdates(new List<PackageUpdateSet>(),
                folder,
                NuGetSources.GlobalFeed, Settings());

            await runner.Received(0)
                .Update(Arg.Any<PackageUpdateSet>(), Arg.Any<NuGetSources>());
        }

        [Test]
        public async Task SingleItemCase()
        {

            var updates = new List<PackageUpdateSet>
            {
                MakePackageUpdateSet("foo")
            };

            var selection = Substitute.For<IUpdateSelection>();
            FilterIsPassThrough(selection);


            var runner = Substitute.For<IUpdateRunner>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();
            var restorer = new SolutionsRestore(Substitute.For<IFileRestoreCommand>());

            var updater = new LocalUpdater(selection, runner, restorer, logger);

            await updater.ApplyUpdates(updates, folder, NuGetSources.GlobalFeed, Settings());

            await runner.Received(1)
                .Update(Arg.Any<PackageUpdateSet>(), Arg.Any<NuGetSources>());
        }

        [Test]
        public async Task TwoItemsCase()
        {

            var updates = new List<PackageUpdateSet>
            {
                MakePackageUpdateSet("foo"),
                MakePackageUpdateSet("bar")
            };

            var selection = Substitute.For<IUpdateSelection>();
            FilterIsPassThrough(selection);

            var runner = Substitute.For<IUpdateRunner>();
            var logger = Substitute.For<INuKeeperLogger>();
            var folder = Substitute.For<IFolder>();
            var restorer = new SolutionsRestore(Substitute.For<IFileRestoreCommand>());

            var updater = new LocalUpdater(selection, runner, restorer, logger);

            await updater.ApplyUpdates(updates, folder, NuGetSources.GlobalFeed, Settings());

            await runner.Received(2)
                .Update(Arg.Any<PackageUpdateSet>(), Arg.Any<NuGetSources>());
        }


        private static void FilterIsPassThrough(IUpdateSelection selection)
        {
            selection
                .Filter(
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<FilterSettings>(),
                    Arg.Any<Func<PackageUpdateSet, Task<bool>>>())
                .Returns(x => x.ArgAt<IReadOnlyCollection<PackageUpdateSet>>(0));
        }

        private static PackageUpdateSet MakePackageUpdateSet(string packageName)
        {
            var fooPackage = new PackageIdentity(packageName, new NuGetVersion("1.2.3"));
            var latest = new PackageSearchMedatadata(fooPackage, new PackageSource("http://none"), null,
                Enumerable.Empty<PackageDependency>());
            var packages = new PackageLookupResult(VersionChange.Major, latest, null, null);

            var path = new PackagePath("c:\\foo", "bar", PackageReferenceType.ProjectFile);
            var pip = new PackageInProject(fooPackage, path, null);

            return new PackageUpdateSet(packages, new List<PackageInProject> {pip});
        }

        private static SettingsContainer Settings()
        {
            return new SettingsContainer
            {
                UserSettings = new UserSettings()
            };
        }
    }
}
