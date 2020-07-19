using NSubstitute;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Git;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.Engine.Packages;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuKeeper.Integration.Tests.Engine
{
    [TestFixture]
    public class ExistingCommitFilterTest : TestWithFailureLogging
    {
        [Test]
        public async Task DoFilter()
        {
            var nugetsToUpdate = new[]
            {
                "First.Nuget",
                "Second.Nuget"
            };

            var nugetsAlreadyCommitted = new[]
            {
                "Second.Nuget",
            };

            var git = MakeGitDriver(nugetsAlreadyCommitted);

            var updates = nugetsToUpdate.Select(n => MakeUpdateSet(n)).ToList();

            var subject = MakeExistingCommitFilter();

            var result = await subject.Filter(git, updates.AsReadOnly(), "base", "head");

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("First.Nuget", result.FirstOrDefault()?.SelectedId);
        }

        [Test]
        public async Task DoNotFilter()
        {
            var nugetsToUpdate = new[]
            {
                "First.Nuget",
                "Second.Nuget"
            };

            var nugetsAlreadyCommitted = new[]
            {
                "Third.Nuget",
            };

            var git = MakeGitDriver(nugetsAlreadyCommitted);

            var updates = nugetsToUpdate.Select(n => MakeUpdateSet(n)).ToList();

            var subject = MakeExistingCommitFilter();

            var result = await subject.Filter(git, updates.AsReadOnly(), "base", "head");

            Assert.AreEqual(2, result.Count);
        }

        private IExistingCommitFilter MakeExistingCommitFilter()
        {
            var collaborationFactory = Substitute.For<ICollaborationFactory>();

            var gitClient = Substitute.For<ICollaborationPlatform>();
            collaborationFactory.CollaborationPlatform.Returns(gitClient);

            var commitWorder = Substitute.For<ICommitWorder>();
            commitWorder.MakeCommitMessage(Arg.Any<PackageUpdateSet>()).Returns(p => $"Automatic update of {((PackageUpdateSet)p[0]).SelectedId} to {((PackageUpdateSet)p[0]).SelectedVersion}");
            collaborationFactory.CommitWorder.Returns(commitWorder);

            return new ExistingCommitFilter(collaborationFactory, NukeeperLogger);
        }

        private static Task<IReadOnlyCollection<string>> FixedReturnVal(string[] ids)
        {
            return Task.Run(() =>
            {
                return (IReadOnlyCollection<string>)ids.Select(id => CreateCommitMessage(id, new NuGetVersion("3.0.0"))).ToList().AsReadOnly();
            });
        }

        private static IGitDriver MakeGitDriver(string[] ids)
        {
            var l = ids.Select(id => CreateCommitMessage(id, new NuGetVersion("3.0.0"))).ToArray();

            var git = Substitute.For<IGitDriver>();
            git.GetNewCommitMessages(Arg.Any<string>(), Arg.Any<string>())
                .Returns(FixedReturnVal(ids));

            return git;
        }

        private static string CreateCommitMessage(string id, NuGetVersion version)
        {
            return $"Automatic update of {id} to {version}";
        }

        private static PackageUpdateSet MakeUpdateSet(string id)
        {
            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject(id, "1.0.0", new PackagePath("base","rel", PackageReferenceType.ProjectFile)),
                new PackageInProject(id, "2.0.0", new PackagePath("base","rel", PackageReferenceType.ProjectFile)),
            };

            var majorUpdate = new PackageSearchMetadata(
                new PackageIdentity(
                    id,
                    new NuGetVersion("3.0.0")),
                new PackageSource("https://api.nuget.org/v3/index.json"), null, null);

            var lookupResult = new PackageLookupResult(VersionChange.Major, majorUpdate, null, null);

            return new PackageUpdateSet(lookupResult, currentPackages);
        }
    }
}
