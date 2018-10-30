using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.RepositoryInspection;
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
            var updates = PackageUpdates.MakeUpdateSet("foo")
                .InList();

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
                PackageUpdates.MakeUpdateSet("foo"),
                PackageUpdates.MakeUpdateSet("bar")
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

        private static SettingsContainer Settings()
        {
            return new SettingsContainer
            {
                UserSettings = new UserSettings()
            };
        }
    }
}
