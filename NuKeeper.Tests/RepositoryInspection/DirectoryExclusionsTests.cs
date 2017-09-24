using NuKeeper.RepositoryInspection;
using NUnit.Framework;

namespace NuKeeper.Tests.RepositoryInspection
{
    [TestFixture]
    public class DirectoryExclusionsTests
    {
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\NuKeeper.csproj")]
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\GitHub\\IGithub.cs")]
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\dustbin.cs")]
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\objections.cs")]
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\Packages.cs")]
        public void ShouldBeIncluded(string path)
        {
            var actual = DirectoryExclusions.PathIsExcluded(path);
            Assert.That(actual, Is.False);
        }

        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\bin\\debug\\net461\\config.json")]
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\obj\\debug\\net461\\config.json")]
        [TestCase("C:\\Code\\NuKeeper\\NuKeeper\\packages\\foo.text")]
        [TestCase("C:\\Code\\NuKeeper\\.git\\config")]
        public void ShouldBeExcluded(string path)
        {
            var actual = DirectoryExclusions.PathIsExcluded(path);
            Assert.That(actual, Is.True);
        }
    }
}
