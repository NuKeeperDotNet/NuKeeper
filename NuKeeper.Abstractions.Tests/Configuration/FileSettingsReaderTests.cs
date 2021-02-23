using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.Output;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IO;

namespace NuKeeper.Abstractions.Tests.Configuration
{
    [TestFixture]
    public class FileSettingsReaderTests
    {
        private string _uniqueTemporaryFolder;

        [SetUp]
        public void Setup()
        {
            _uniqueTemporaryFolder = UniqueTemporaryFolder();
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_uniqueTemporaryFolder))
            {
                Directory.Delete(_uniqueTemporaryFolder, true);
            }
        }



        [Test]
        public void MissingFileReturnsNoSettings()
        {
            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(_uniqueTemporaryFolder);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.Null);
            Assert.That(data.Api, Is.Null);
            Assert.That(data.Include, Is.Null);
            Assert.That(data.Exclude, Is.Null);
            Assert.That(data.Label, Is.Null);
            Assert.That(data.MaxPackageUpdates, Is.Null);
            Assert.That(data.MaxOpenPullRequests, Is.Null);
            Assert.That(data.MaxRepo, Is.Null);
            Assert.That(data.Verbosity, Is.Null);
            Assert.That(data.Change, Is.Null);
            Assert.That(data.ForkMode, Is.Null);
            Assert.That(data.UsePrerelease, Is.Null);

            Assert.That(data.OutputDestination, Is.Null);
            Assert.That(data.OutputFormat, Is.Null);
            Assert.That(data.OutputFileName, Is.Null);
            Assert.That(data.LogDestination, Is.Null);
            Assert.That(data.Platform, Is.Null);
            Assert.That(data.BranchNameTemplate, Is.Null);
            Assert.That(data.DeleteBranchAfterMerge, Is.Null);
        }

        [Test]
        public void EmptyConfigReturnsNoSettings()
        {
            var path = MakeTestFile("{}");

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.Null);
            Assert.That(data.Api, Is.Null);
            Assert.That(data.Include, Is.Null);
            Assert.That(data.Exclude, Is.Null);
            Assert.That(data.Label, Is.Null);
            Assert.That(data.MaxPackageUpdates, Is.Null);
            Assert.That(data.MaxOpenPullRequests, Is.Null);
            Assert.That(data.MaxRepo, Is.Null);
            Assert.That(data.Verbosity, Is.Null);
            Assert.That(data.Change, Is.Null);
            Assert.That(data.ForkMode, Is.Null);
            Assert.That(data.UsePrerelease, Is.Null);

            Assert.That(data.OutputDestination, Is.Null);
            Assert.That(data.OutputFormat, Is.Null);
            Assert.That(data.OutputFileName, Is.Null);
            Assert.That(data.LogDestination, Is.Null);
            Assert.That(data.Platform, Is.Null);
            Assert.That(data.BranchNameTemplate, Is.Null);
            Assert.That(data.DeleteBranchAfterMerge, Is.Null);
        }

        private const string FullFileData = @"{
               ""age"":""3d"",
               ""api"":""http://api.com"",
               ""include"":""fred"",
               ""exclude"":""fish"",
               ""includeRepos"":""repoIn"",
               ""excludeRepos"":""repoOut"",
               ""label"": [ ""foo"", ""bar"" ],
               ""logFile"":""somefile.log"",
               ""branchNameTemplate"": ""nukeeper/MyBranch"",
               ""maxPackageUpdates"": 42,
               ""maxOpenPullRequests"": 10,
               ""maxRepo"": 12,
               ""verbosity"": ""Detailed"",
               ""Change"": ""Minor"",
               ""ForkMode"": ""PreferFork"",
               ""UsePrerelease"": ""Never"",
               ""OutputFormat"": ""Text"",
               ""OutputDestination"": ""Console"",
               ""OutputFileName"" : ""out_42.txt"",
               ""LogDestination"" : ""file"",
               ""Platform"" : ""Bitbucket"",
               ""DeleteBranchAfterMerge"": ""true""
        }";

        [Test]
        public void PopulatedConfigReturnsAllStringSettings()
        {
            var path = MakeTestFile(FullFileData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
            Assert.That(data.Include, Is.EqualTo("fred"));
            Assert.That(data.Exclude, Is.EqualTo("fish"));
            Assert.That(data.IncludeRepos, Is.EqualTo("repoIn"));
            Assert.That(data.ExcludeRepos, Is.EqualTo("repoOut"));
            Assert.That(data.LogFile, Is.EqualTo("somefile.log"));
            Assert.That(data.OutputFileName, Is.EqualTo("out_42.txt"));
            Assert.That(data.BranchNameTemplate, Is.EqualTo("nukeeper/MyBranch"));
            Assert.That(data.DeleteBranchAfterMerge, Is.EqualTo(true));
        }

        [Test]
        public void PopulatedConfigReturnsLabels()
        {
            var path = MakeTestFile(FullFileData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data.Label.Count, Is.EqualTo(2));
            Assert.That(data.Label, Does.Contain("foo"));
            Assert.That(data.Label, Does.Contain("bar"));
        }

        [Test]
        public void PopulatedConfigReturnsNumericSettings()
        {
            var path = MakeTestFile(FullFileData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data.MaxPackageUpdates, Is.EqualTo(42));
            Assert.That(data.MaxOpenPullRequests, Is.EqualTo(10));
            Assert.That(data.MaxRepo, Is.EqualTo(12));
        }

        [Test]
        public void PopulatedConfigReturnsEnumSettings()
        {
            var path = MakeTestFile(FullFileData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data.Change, Is.EqualTo(VersionChange.Minor));
            Assert.That(data.ForkMode, Is.EqualTo(ForkMode.PreferFork));
            Assert.That(data.UsePrerelease, Is.EqualTo(UsePrerelease.Never));

            Assert.That(data.Verbosity, Is.EqualTo(LogLevel.Detailed));
            Assert.That(data.LogDestination, Is.EqualTo(LogDestination.File));

            Assert.That(data.OutputDestination, Is.EqualTo(OutputDestination.Console));
            Assert.That(data.OutputFormat, Is.EqualTo(OutputFormat.Text));
            Assert.That(data.Platform, Is.EqualTo(Platform.Bitbucket));
        }

        [Test]
        public void ConfigKeysAreCaseInsensitive()
        {
            const string configData = @"{
               ""Age"":""3d"",
               ""API"":""http://api.com"",
               ""iNClude"":""fred"",
               ""excludE"":""fish"",
               ""IncluDeRepoS"":""repo2"",
               ""label"": [""mark"" ],
               ""MaxPackageUpdates"":4,
               ""MaxOpenPUllrequests"":10,
               ""MAXrepo"":3,
               ""vErBoSiTy"": ""Q"",
               ""CHANGE"": ""PATCH"",
               ""bRanCHNamETempLATe"": ""nukeeper/MyBranch"",
               ""deLeTEBranCHafTERMerge"": ""true""
            }";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
            Assert.That(data.Include, Is.EqualTo("fred"));
            Assert.That(data.Exclude, Is.EqualTo("fish"));
            Assert.That(data.IncludeRepos, Is.EqualTo("repo2"));
            Assert.That(data.Label.Count, Is.EqualTo(1));
            Assert.That(data.Label, Does.Contain("mark"));
            Assert.That(data.MaxPackageUpdates, Is.EqualTo(4));
            Assert.That(data.MaxOpenPullRequests, Is.EqualTo(10));
            Assert.That(data.MaxRepo, Is.EqualTo(3));
            Assert.That(data.Verbosity, Is.EqualTo(LogLevel.Quiet));
            Assert.That(data.Change, Is.EqualTo(VersionChange.Patch));
            Assert.That(data.BranchNameTemplate, Is.EqualTo("nukeeper/MyBranch"));
            Assert.That(data.DeleteBranchAfterMerge, Is.EqualTo(true));
        }

        [Test]
        public void ExtraKeysAreIgnored()
        {
            const string configData = @"{
               ""age"":""3d"",
               ""api"":""http://api.com"",
               ""something"":""nothing""
            }";

            var path = MakeTestFile(configData);

            var fsr = new FileSettingsReader(Substitute.For<INuKeeperLogger>());

            var data = fsr.Read(path);

            Assert.That(data, Is.Not.Null);
            Assert.That(data.Age, Is.EqualTo("3d"));
            Assert.That(data.Api, Is.EqualTo("http://api.com"));
        }

        private string MakeTestFile(string contents)
        {
            var path = Path.Join(_uniqueTemporaryFolder, "nukeeper.settings.json");
            File.WriteAllText(path, contents);
            return _uniqueTemporaryFolder;
        }

        private static string UniqueTemporaryFolder()
        {
            var uniqueName = Guid.NewGuid().ToString("N", CultureInfo.InvariantCulture);
            var folder = Path.Combine(Path.GetTempPath(), "NuKeeper", uniqueName);

            var tempDir = new DirectoryInfo(folder);
            tempDir.Create();

            return folder;
        }
    }
}
