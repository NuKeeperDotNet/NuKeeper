using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Github;
using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class TargetSelectionTests
    {
        [Test]
        public void NoneIn_NoneOut()
        {
            var updateSets = new List<PackageUpdateSet>();

            var target = OneTargetSelection();

            var results = target.SelectTargets(updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void OneIn_OneOut()
        {
            var updateSets = new List<PackageUpdateSet> { UpdateFooInOneProject() };

            var target = OneTargetSelection();

            var results = target.SelectTargets(updateSets);

            Assert.That(results, Is.Not.Null);
            Assert.That(results.Count, Is.EqualTo(1));
        }

        private PackageUpdateSet UpdateFooInOneProject()
        {
            var newPackage = LatestVersionOfPackageFoo();

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne())
            };

            return new PackageUpdateSet(newPackage, currentPackages);
        }

        private PackageIdentity LatestVersionOfPackageFoo()
        {
            return new PackageIdentity("foo", new NuGetVersion("1.2.3"));
        }

        private PackagePath PathToProjectOne()
        {
            return new PackagePath("c_temp", "projectOne", PackageReferenceType.PackagesConfig);
        }
        private static TargetSelection OneTargetSelection()
        {
            var repo = new GithubRepository
            {
                HtmlUrl = "https://foo.com",
                Name = "test",
                Owner = new GithubOwner
                {
                    Login = "test"
                }
            };
            var settings = new RepositoryModeSettings(repo, null, string.Empty, 1);

            return new TargetSelection(settings);
        }
    }
}
