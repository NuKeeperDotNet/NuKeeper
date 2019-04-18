using NuKeeper.Abstractions.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Inspection.Tests.RepositoryInspection
{
    public static class PackageAssert
    {
        public static void IsPopulated(PackageInProject package)
        {
            Assert.That(package, Is.Not.Null);
            Assert.That(package.PackageVersionRange, Is.Not.Null);
            Assert.That(package.Version, Is.Not.Null);
            Assert.That(package.Identity, Is.Not.Null);
            Assert.That(package.Path, Is.Not.Null);

            Assert.That(package.Id, Is.Not.Empty);
            Assert.That(package.Version.ToString(), Is.Not.Empty);
            Assert.That(package.ProjectReferences, Is.Not.Null);
        }
    }
}
