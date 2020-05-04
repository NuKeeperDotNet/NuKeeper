using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Report;
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
            var finder = Substitute.For<IUpdateFinder>();
            var updater = Substitute.For<ILocalUpdater>();
            var engine = MakeLocalEngine(finder, updater);

            var settings = new SettingsContainer
            {
                UserSettings = new UserSettings()
            };

            await engine.Run(settings, false);

            await finder.Received()
                .FindPackageUpdateSets(Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>(),
                    Arg.Any<UsePrerelease>());

            await updater.Received(0)
                .ApplyUpdates(
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<SettingsContainer>());
        }

        [Test]
        public async Task CanRunUpdate()
        {
            var finder = Substitute.For<IUpdateFinder>();
            var updater = Substitute.For<ILocalUpdater>();
            var engine = MakeLocalEngine(finder, updater);

            var settings = new SettingsContainer
            {
                UserSettings = new UserSettings()
            };

            await engine.Run(settings, true);

            await finder.Received()
                .FindPackageUpdateSets(Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>(),
                    Arg.Any<UsePrerelease>());

            await updater
                .Received(1)
                .ApplyUpdates(
                    Arg.Any<IReadOnlyCollection<PackageUpdateSet>>(),
                    Arg.Any<IFolder>(),
                    Arg.Any<NuGetSources>(),
                    Arg.Any<SettingsContainer>());
        }

        private static LocalEngine MakeLocalEngine(IUpdateFinder finder, ILocalUpdater updater)
        {
            var reader = Substitute.For<INuGetSourcesReader>();
            finder.FindPackageUpdateSets(
                    Arg.Any<IFolder>(), Arg.Any<NuGetSources>(),
                    Arg.Any<VersionChange>(),
                    Arg.Any<UsePrerelease>())
                .Returns(new List<PackageUpdateSet>());

            var sorter = Substitute.For<IPackageUpdateSetSort>();
            sorter.Sort(Arg.Any<IReadOnlyCollection<PackageUpdateSet>>())
                .Returns(x => x.ArgAt<IReadOnlyCollection<PackageUpdateSet>>(0));

            var logger = Substitute.For<INuKeeperLogger>();

            var nugetLogger = Substitute.For<NuGet.Common.ILogger>();

            var reporter = Substitute.For<IReporter>();
            
            var engine = new LocalEngine(reader, finder, sorter, updater,
                reporter, logger, nugetLogger);
            Assert.That(engine, Is.Not.Null);
            return engine;
        }
    }
}
