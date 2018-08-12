using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Creators;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Files;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sort;
using NuKeeper.Inspection.Sources;
using NuKeeper.Local;
using NUnit.Framework;

namespace NuKeeper.Tests.Local
{
    [TestFixture]
    public class LocalEngineTests
    {
        [Test]
        public async Task CanRunInspect()
        {
            var updater = Substitute.For<ILocalUpdater>();
            var engine = MakeLocalEngine(updater);

            var settings = new SettingsContainer
            {
                ModalSettings = new ModalSettings
                {
                    Mode = RunMode.Inspect
                },
                UserSettings = new UserSettings()
            };

            await engine.Run(settings);

            await updater
                .Received(0)
                .ApplyUpdates(
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<NuGetSources>());
        }

        [Test]
        public async Task CanRunUpdate()
        {
            var updater = Substitute.For<ILocalUpdater>();
            var engine = MakeLocalEngine(updater);

            var settings = new SettingsContainer
            {
                ModalSettings = new ModalSettings
                {
                    Mode = RunMode.Update
                },
                UserSettings = new UserSettings()
            };

            await engine.Run(settings);

            await updater
                .Received(1)
                .ApplyUpdates(
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<NuGetSources>());
        }

        private static LocalEngine MakeLocalEngine(ILocalUpdater updater)
        {
            var reader = Substitute.For<INuGetSourcesReader>();
            var finder = Substitute.For<IUpdateFinder>();
            finder.FindPackageUpdateSets(
                    Arg.Any<IFolder>(), Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>())
                .Returns(new List<PackageUpdateSet>());

            var sorter = Substitute.For<IPackageUpdateSetSort>();
            sorter.Sort(Arg.Any<IReadOnlyCollection<PackageUpdateSet>>())
                .Returns(x => x.ArgAt<IReadOnlyCollection<PackageUpdateSet>>(0));

            var logger = Substitute.For<INuKeeperLogger>();

            var updaterCreator = Substitute.For<ICreate<ILocalUpdater>>();
            updaterCreator.Create(Arg.Any<SettingsContainer>())
                .Returns(updater);

            var engine = new LocalEngine(reader, finder, sorter, updaterCreator, logger);
            Assert.That(engine, Is.Not.Null);
            return engine;
        }
    }
}
