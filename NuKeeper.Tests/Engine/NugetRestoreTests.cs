using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class NugetRestoreTests
    {
        [Test]
        public async Task WhenNugetRestoreIsCalledThenArgsIncludePackageDirectory()
        {
            var logger = Substitute.For<INuKeeperLogger>();
            var nuGetPath = Substitute.For<INuGetPath>();           
            var monoExecuter = Substitute.For<IMonoExecutor>();
            var externalProcess = Substitute.For<IExternalProcess>();
            var file = new FileInfo("packages.config");           
            nuGetPath.Executable.Returns(@"c:\DoesNotExist\nuget.exe");
            externalProcess.Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>()).Returns(new ProcessOutput("","",0));
            var cmd = new NuGetFileRestoreCommand(logger, nuGetPath, monoExecuter, externalProcess);
            await cmd.Invoke(file, NuGetSources.GlobalFeed);

            await externalProcess.Received(1).Run(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<bool>());
            await externalProcess.ReceivedWithAnyArgs().Run(Arg.Any<string>(), Arg.Any<string>(), $"restore {file.Name} - Source ${NuGetSources.GlobalFeed} -NonInteractive -PackagesDirectory ..\\packages", Arg.Any<bool>());
        }
    }
}
