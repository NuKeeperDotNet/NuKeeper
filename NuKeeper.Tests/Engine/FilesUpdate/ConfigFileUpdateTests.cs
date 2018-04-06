using NUnit.Framework;
using NuKeeper.Engine.FilesUpdate;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.Tests.Engine.FilesUpdate
{
    [TestFixture]
    public class ConfigFileUpdateTests
    {
        const string WebConfigFileContents = @"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""System.Web.Http"" publicKeyToken=""31bf3856ad364e35"" culture=""neutral"" />
        <bindingRedirect oldVersion=""0.0.0.0-5.2.3.0"" newVersion=""5.2.3.0"" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name=""System.Net.Http.Formatting"" publicKeyToken=""31bf3856ad364e35"" culture=""neutral"" />
        <bindingRedirect oldVersion=""0.0.0.0-5.2.3.0"" newVersion=""5.2.3.0"" />
      </dependentAssembly>
  </runtime>
</configuration>";

        [Test]
        public void ShouldReturnNullForNullInput()
        {
            var from = MakePackageVersion("foo.bar", "1.2.3");
            var to = MakePackageVersion("foo.bar", "2.3.4");
            var updater = new ConfigFileUpdater(from, to);

            var result = updater.ApplyUpdate(null);

            Assert.That(result, Is.Null);
        }

        [Test]
        public void ShouldReturnInputForNoMatchInPlainText()
        {
            var from = MakePackageVersion("foo.bar", "1.2.3");
            var to = MakePackageVersion("foo.bar", "2.3.4");
            var updater = new ConfigFileUpdater(from, to);
            var input = "Test text, no match.";

            var result = updater.ApplyUpdate(input);

            Assert.That(result, Is.EqualTo(input));
        }

        [Test]
        public void ShouldReturnInputForNoMatchInConfig()
        {
            var from = MakePackageVersion("foo.bar", "1.2.3");
            var to = MakePackageVersion("foo.bar", "2.3.4");
            var updater = new ConfigFileUpdater(from, to);

            var result = updater.ApplyUpdate(WebConfigFileContents);

            Assert.That(result, Is.EqualTo(WebConfigFileContents));
        }

        [Test]
        public void ShouldPerformMatchingUpdate()
        {
            var from = MakePackageVersion("System.Net.Http.Formatting", "5.2.3.0");
            var to = MakePackageVersion("System.Net.Http.Formatting", "6.1.2.3");
            var updater = new ConfigFileUpdater(from, to);

            var result = updater.ApplyUpdate(WebConfigFileContents);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("<assemblyIdentity name=\"System.Net.Http.Formatting\" "));
            Assert.That(result, Does.Contain("oldVersion=\"0.0.0.0-6.1.2.3\""));
            Assert.That(result, Does.Contain("newVersion=\"6.1.2.3\""));
        }

        [Test]
        public void ShouldNotPerformUpdateWhenVersionDoesNotMatch()
        {
            var from = MakePackageVersion("System.Net.Http.Formatting", "5.2.4.0");
            var to = MakePackageVersion("System.Net.Http.Formatting", "6.1.2.3");
            var updater = new ConfigFileUpdater(from, to);

            var result = updater.ApplyUpdate(WebConfigFileContents);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain("<assemblyIdentity name=\"System.Net.Http.Formatting\" "));
            Assert.That(result, Does.Contain("oldVersion=\"0.0.0.0-5.2.3.0\""));
            Assert.That(result, Does.Contain("newVersion=\"5.2.3.0\""));
        }

        private PackageIdentity MakePackageVersion(string name, string version)
        {
            return new PackageIdentity(name, new NuGetVersion(version));
        }
    }
}
