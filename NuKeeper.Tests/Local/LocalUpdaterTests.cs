using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.RepositoryInspection;
using NuKeeper.Inspection.Sources;
using NuKeeper.Local;
using NuKeeper.Update;
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

            var updater = new LocalUpdater(selection, runner, logger);

            await updater.ApplyUpdates(new List<PackageUpdateSet>(), NuGetSources.GlobalFeed);

            await runner.Received(0)
                .Update(Arg.Any<PackageUpdateSet>(), Arg.Any<NuGetSources>());
        }
    }
}
