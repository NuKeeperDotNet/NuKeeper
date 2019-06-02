using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NuKeeper.Collaboration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NUnit.Framework;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class CollaborationFactoryTests
    {
        private static CollaborationFactory GetCollaborationFactory()
        {
            var azureUri = new Uri("https://dev.azure.com");
            var gitHubUri = new Uri("https://api.github.com");

            var settingReader1 = Substitute.For<ISettingsReader>();
            settingReader1.CanRead(azureUri).Returns(true);
            settingReader1.Platform.Returns(Platform.AzureDevOps);

            var settingReader2 = Substitute.For<ISettingsReader>();
            settingReader2.CanRead(gitHubUri).Returns(true);
            settingReader2.Platform.Returns(Platform.GitHub);

            var readers = new List<ISettingsReader> { settingReader1, settingReader2 };
            var logger = Substitute.For<INuKeeperLogger>();
            return new CollaborationFactory(readers, logger);
        }

        [Test]
        public void UnitialisedFactoryHasNulls()
        {
            var f = GetCollaborationFactory();

            Assert.That(f, Is.Not.Null);
            Assert.That(f.CollaborationPlatform, Is.Null);
            Assert.That(f.ForkFinder, Is.Null);
            Assert.That(f.RepositoryDiscovery, Is.Null);
        }

        [Test]
        public async Task UnknownApiReturnsUnableToFindPlatform()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                    new Uri("https://unknown.com/"), null,
                    ForkMode.SingleRepositoryOnly, null);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage,
                Is.EqualTo("Unable to find collaboration platform for uri https://unknown.com/"));
        }

        [Test]
        public async Task UnknownApiCanHaveManualPlatform()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                    new Uri("https://unknown.com/"), "token",
                    ForkMode.SingleRepositoryOnly,
                    Platform.GitHub);

            Assert.That(result.IsSuccess);
            AssertGithub(collaborationFactory);
        }

        [Test]
        public async Task ManualPlatformWillOverrideUri()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.myco.com"), "token",
                ForkMode.SingleRepositoryOnly,
                Platform.AzureDevOps);

            Assert.That(result.IsSuccess);
            AssertAzureDevOps(collaborationFactory);
        }

        [Test]
        public async Task AzureDevOpsUrlReturnsAzureDevOps()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(new Uri("https://dev.azure.com"), "token",
                ForkMode.SingleRepositoryOnly, null);
            Assert.That(result.IsSuccess);

            AssertAzureDevOps(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        [Test]
        public async Task GithubUrlReturnsGitHub()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(new Uri("https://api.github.com"), "token",
                ForkMode.PreferFork, null);
            Assert.That(result.IsSuccess);

            AssertGithub(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        private static void AssertAreSameObject(ICollaborationFactory collaborationFactory)
        {
            var collaborationPlatform = collaborationFactory.CollaborationPlatform;
            Assert.AreSame(collaborationPlatform, collaborationFactory.CollaborationPlatform);

            var repositoryDiscovery = collaborationFactory.RepositoryDiscovery;
            Assert.AreSame(repositoryDiscovery, collaborationFactory.RepositoryDiscovery);

            var forkFinder = collaborationFactory.ForkFinder;
            Assert.AreSame(forkFinder, collaborationFactory.ForkFinder);

            var settings = collaborationFactory.Settings;
            Assert.AreSame(settings, collaborationFactory.Settings);
        }

        private static void AssertGithub(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<GitHubForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<GitHubRepositoryDiscovery>(collaborationFactory.RepositoryDiscovery);
            Assert.IsInstanceOf<OctokitClient>(collaborationFactory.CollaborationPlatform);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
            Assert.IsInstanceOf<DefaultCommitWorder>(collaborationFactory.CommitWorder);
        }

        private static void AssertAzureDevOps(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<AzureDevOpsForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<AzureDevOpsRepositoryDiscovery>(collaborationFactory.RepositoryDiscovery);
            Assert.IsInstanceOf<AzureDevOpsPlatform>(collaborationFactory.CollaborationPlatform);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
            Assert.IsInstanceOf<AzureDevOpsCommitWorder>(collaborationFactory.CommitWorder);
        }
    }
}
