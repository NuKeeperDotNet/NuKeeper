using System.IO;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Engine;
using NuKeeper.Files;
using NuKeeper.NuGet.Process;
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

            var solutionResture = new SolutionsRestore(cmd);

            await solutionResture.Restore(folder);

            await cmd.DidNotReceiveWithAnyArgs().Invoke(Arg.Any<FileInfo>());
        }

        [Test]
        public async Task WhenThereIsOneSolutionsTheCommandIsCalled()
        {
            var sln = new FileInfo("foo.sln");

            var cmd = Substitute.For<IFileRestoreCommand>();
            var folder = Substitute.For<IFolder>();
            folder.Find(Arg.Any<string>()).Returns(new[] { sln });

            var solutionResture = new SolutionsRestore(cmd);

            await solutionResture.Restore(folder);

            await cmd.Received(1).Invoke(Arg.Any<FileInfo>());
            await cmd.Received().Invoke(sln);
        }

        [Test]
        public async Task WhenThereAreTwoSolutionsTheCommandIsCalledForEachOfThem()
        {
            var sln1 = new FileInfo("foo.sln");
            var sln2 = new FileInfo("bar.sln");

            var cmd = Substitute.For<IFileRestoreCommand>();
            var folder = Substitute.For<IFolder>();
            folder.Find(Arg.Any<string>()).Returns(new[] { sln1, sln2 });

            var solutionResture = new SolutionsRestore(cmd);

            await solutionResture.Restore(folder);

            await cmd.Received(2).Invoke(Arg.Any<FileInfo>());
            await cmd.Received().Invoke(sln1);
            await cmd.Received().Invoke(sln2);
        }
    }
}
