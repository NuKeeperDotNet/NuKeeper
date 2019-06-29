using System;
using System.Collections.Generic;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGetApi;
using NuKeeper.Abstractions.RepositoryInspection;
using NSubstitute;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Update.Process;
using NUnit.Framework;
using NuKeeper.Abstractions.NuGet;
using System.Threading.Tasks;

namespace NuKeeper.Update.Tests
{
    [TestFixture]
    public class UpdateRunnerTests
    {
        private INuKeeperLogger _nuKeeperLogger;
        private ISettingsContainer _settingsContainer;
        private IFileRestoreCommand _fileRestoreCommand;
        private INuGetUpdatePackageCommand _nuGetUpdatePackageCommand;
        private IDotNetUpdatePackageCommand _dotNetUpdatePackageCommand;
        private IDotNetRestoreCommand _dotNetRestoreCommand;
        private IUpdateProjectImportsCommand _updateProjectImportsCommand;
        private IUpdateNuspecCommand _updateNuspecCommand;
        private IUpdateDirectoryBuildTargetsCommand _updateDirectoryBuildTargetsCommand;
        private IUpdateRunner _sut;

        [OneTimeSetUp]
        public void SetUp()
        {
            _nuKeeperLogger = Substitute.For<INuKeeperLogger>();

            _settingsContainer = Substitute.For<ISettingsContainer>();
            _settingsContainer.UserSettings = new UserSettings();

            _fileRestoreCommand = Substitute.For<IFileRestoreCommand>();
            _nuGetUpdatePackageCommand = Substitute.For<INuGetUpdatePackageCommand>();
            _dotNetUpdatePackageCommand = Substitute.For<IDotNetUpdatePackageCommand>();
            _dotNetRestoreCommand = Substitute.For<IDotNetRestoreCommand>();
            _updateProjectImportsCommand = Substitute.For<IUpdateProjectImportsCommand>();
            _updateNuspecCommand = Substitute.For<IUpdateNuspecCommand>();
            _updateDirectoryBuildTargetsCommand = Substitute.For<IUpdateDirectoryBuildTargetsCommand>();

            _sut = new UpdateRunner(
                _nuKeeperLogger,
                _settingsContainer,
                _fileRestoreCommand,
                _nuGetUpdatePackageCommand,
                _dotNetUpdatePackageCommand,
                _dotNetRestoreCommand,
                _updateProjectImportsCommand,
                _updateNuspecCommand,
                _updateDirectoryBuildTargetsCommand);
        }

        [TestCase(true, Description = "Should perform dotnet restore's before each package update.")]
        [TestCase(false, Description = "Should not perform dotnet restore's before each package update.")]
        public async Task CorrectCommandsAreExecutedForProjectFileUpdate(bool restoreBeforePackageUpdate)
        {
            // Arrange
            _settingsContainer.UserSettings.RestoreBeforePackageUpdate = restoreBeforePackageUpdate;
            var packageReferenceType = PackageReferenceType.ProjectFile;
            var packageUpdateSet = UpdateFooFromOneVersion(packageReferenceType);
            var sources = NuGetSources.GlobalFeed;

            // Act
            await _sut.Update(packageUpdateSet, sources);

            // Assert
            await AssertCorrectProjectFileCommandsAreExecuted(packageUpdateSet, sources, restoreBeforePackageUpdate);
        }

        [TestCase(true, Description = "Should perform dotnet restore's before each package update.")]
        [TestCase(false, Description = "Should not perform dotnet restore's before each package update.")]
        public async Task CorrectCommandsAreExecutedForProjectFileOldStyleUpdate(bool restoreBeforePackageUpdate)
        {
            // Arrange
            _settingsContainer.UserSettings.RestoreBeforePackageUpdate = restoreBeforePackageUpdate;
            var packageReferenceType = PackageReferenceType.ProjectFileOldStyle;
            var packageUpdateSet = UpdateFooFromOneVersion(packageReferenceType);
            var sources = NuGetSources.GlobalFeed;

            // Act
            await _sut.Update(packageUpdateSet, sources);

            // Assert
            await AssertCorrectProjectFileOldStyleCommandsAreExecuted(packageUpdateSet, sources, restoreBeforePackageUpdate);
        }

        [Test]
        public async Task CorrectCommandsAreExecutedForPackagesConfigUpdate()
        {
            // Arrange
            var packageReferenceType = PackageReferenceType.PackagesConfig;
            var packageUpdateSet = UpdateFooFromOneVersion(packageReferenceType);
            var sources = NuGetSources.GlobalFeed;

            // Act
            await _sut.Update(packageUpdateSet, sources);

            // Assert
            await AssertCorrectPackagesConfigCommandsAreExecuted(packageUpdateSet, sources);
        }

        [Test]
        public async Task CorrectCommandsAreExecutedForNuspecUpdate()
        {
            // Arrange
            var packageReferenceType = PackageReferenceType.Nuspec;
            var packageUpdateSet = UpdateFooFromOneVersion(packageReferenceType);
            var sources = NuGetSources.GlobalFeed;

            // Act
            await _sut.Update(packageUpdateSet, sources);

            // Assert
            await AssertCorrectNuspecCommandsAreExecuted(packageUpdateSet, sources);
        }

        [Test]
        public async Task CorrectCommandsAreExecutedForDirectoryBuildTargetsUpdate()
        {
            // Arrange
            var packageReferenceType = PackageReferenceType.DirectoryBuildTargets;
            var packageUpdateSet = UpdateFooFromOneVersion(packageReferenceType);
            var sources = NuGetSources.GlobalFeed;

            // Act
            await _sut.Update(packageUpdateSet, sources);

            // Assert
            await AssertCorrectDirectoryBuildTargetsCommandsAreExecuted(packageUpdateSet, sources);
        }

        private async Task AssertCorrectProjectFileCommandsAreExecuted(PackageUpdateSet packageUpdateSet, NuGetSources sources, bool restoreBeforePackageUpdate)
        {
            if (!restoreBeforePackageUpdate)
            {
                await Task.WhenAll(
                    _dotNetUpdatePackageCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                    _dotNetRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), sources));
                return;
            }

            await Task.WhenAll(
                _dotNetUpdatePackageCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                _dotNetRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count * 2).Invoke(Arg.Any<PackageInProject>(), sources));
        }

        private async Task AssertCorrectProjectFileOldStyleCommandsAreExecuted(PackageUpdateSet packageUpdateSet, NuGetSources sources, bool restoreBeforePackageUpdate)
        {
            if (!restoreBeforePackageUpdate)
            {
                await Task.WhenAll(
                    _updateProjectImportsCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                    _fileRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                    _dotNetUpdatePackageCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                    _dotNetRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), sources));
                return;
            }

            await Task.WhenAll(
                _updateProjectImportsCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                _fileRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                _dotNetUpdatePackageCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                _dotNetRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count * 2).Invoke(Arg.Any<PackageInProject>(), sources));
        }

        private async Task AssertCorrectPackagesConfigCommandsAreExecuted(PackageUpdateSet packageUpdateSet, NuGetSources sources)
        {
            await Task.WhenAll(
                _fileRestoreCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources),
                _nuGetUpdatePackageCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources));
        }

        private async Task AssertCorrectNuspecCommandsAreExecuted(PackageUpdateSet packageUpdateSet, NuGetSources sources)
        {
            await _updateNuspecCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources);
        }

        private async Task AssertCorrectDirectoryBuildTargetsCommandsAreExecuted(PackageUpdateSet packageUpdateSet, NuGetSources sources)
        {
            await _updateDirectoryBuildTargetsCommand.Received(packageUpdateSet.CurrentPackages.Count).Invoke(Arg.Any<PackageInProject>(), Arg.Any<NuGetVersion>(), Arg.Any<PackageSource>(), sources);
        }

        private static PackageUpdateSet UpdateFooFromOneVersion(PackageReferenceType packageReferenceType, TimeSpan? packageAge = null)
        {
            var pubDate = DateTimeOffset.Now.Subtract(packageAge ?? TimeSpan.Zero);

            var currentPackages = new List<PackageInProject>
            {
                new PackageInProject("foo", "1.0.1", PathToProjectOne(packageReferenceType)),
                new PackageInProject("foo", "1.0.1", PathToProjectTwo(packageReferenceType))
            };

            var matchVersion = new NuGetVersion("4.0.0");
            var match = new PackageSearchMetadata(new PackageIdentity("foo", matchVersion),
                new PackageSource("http://none"), pubDate, null);

            var updates = new PackageLookupResult(VersionChange.Major, match, null, null);
            return new PackageUpdateSet(updates, currentPackages);
        }

        private static PackagePath PathToProjectOne(PackageReferenceType packageReferenceType)
        {
            return new PackagePath("c_temp", "projectOne", packageReferenceType);
        }

        private static PackagePath PathToProjectTwo(PackageReferenceType packageReferenceType)
        {
            return new PackagePath("c_temp", "projectTwo", packageReferenceType);
        }
    }
}
