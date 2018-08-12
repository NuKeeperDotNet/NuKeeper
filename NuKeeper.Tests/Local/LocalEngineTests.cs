using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Configuration;
using NuKeeper.Creators;
using NuKeeper.Inspection;
using NuKeeper.Inspection.Logging;
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
            var engine = MakeLocalEngine();

            var settings = new SettingsContainer
            {
                ModalSettings = new ModalSettings
                {
                    Mode = RunMode.Inspect
                },
                UserSettings = new UserSettings()
            };

            await engine.Run(settings);
        }

        private static LocalEngine MakeLocalEngine()
        {
            var reader = Substitute.For<INuGetSourcesReader>();
            var finder = Substitute.For<IUpdateFinder>();
            var sorter = Substitute.For<IPackageUpdateSetSort>();
            var logger = Substitute.For<INuKeeperLogger>();

            var updater = Substitute.For<ICreate<ILocalUpdater>>();

            var engine = new LocalEngine(reader, finder, sorter, updater, logger);
            Assert.That(engine, Is.Not.Null);
            return engine;
        }
    }
}
