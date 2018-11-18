using System;
using System.Collections.Generic;
using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NuKeeper.Collaboration;
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

            var readers = new List<ISettingsReader> {settingReader1, settingReader2};
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
        public void UnknownApiReturnsUnableToFindPlatform()
        {
            var collaborationFactory = GetCollaborationFactory();

            var exception = Assert.Throws<NuKeeperException>(
                () => collaborationFactory.Initialise(
                    new Uri("https://unknown.com/"), null, ForkMode.SingleRepositoryOnly));

            Assert.AreEqual(exception.Message, "Unable to find collaboration platform for uri https://unknown.com/");
        }

        [Test]
        public void AzureDevOpsUrlReturnsAzureDevOps()
        {
            var collaborationFactory = GetCollaborationFactory();

            collaborationFactory.Initialise(new Uri("https://dev.azure.com"), "token", ForkMode.SingleRepositoryOnly);

            AssertAzureDevOps(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        [Test]
        public void GithubUrlReturnsGitHub()
        {
            var collaborationFactory = GetCollaborationFactory();

            collaborationFactory.Initialise(new Uri("https://api.github.com"), "token", ForkMode.PreferFork);

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
        }

        private static void AssertAzureDevOps(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<AzureDevOpsForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<AzureDevOpsRepositoryDiscovery>(collaborationFactory.RepositoryDiscovery);
            Assert.IsInstanceOf<AzureDevOpsPlatform>(collaborationFactory.CollaborationPlatform);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
        }
    }
}
