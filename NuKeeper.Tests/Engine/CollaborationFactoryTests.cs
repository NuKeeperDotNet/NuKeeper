using System;
using System.Collections.Generic;
using NSubstitute;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
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
            settingReader1.AuthSettings(Arg.Any<Uri>(), Arg.Any<string>()).Returns(new AuthSettings(azureUri, "token"));

            var settingReader2 = Substitute.For<ISettingsReader>();
            settingReader2.CanRead(gitHubUri).Returns(true);
            settingReader2.Platform.Returns(Platform.GitHub);
            settingReader2.AuthSettings(Arg.Any<Uri>(), Arg.Any<string>()).Returns(new AuthSettings(gitHubUri, "token"));


            var readers = new List<ISettingsReader> {settingReader1, settingReader2};
            var logger = Substitute.For<INuKeeperLogger>();
            var platform = Substitute.For<ICollaborationPlatform>();
            return new CollaborationFactory(readers, platform, logger);
        }

        [Test]
        public void NoInitialiseReturnsEmptyProps()
        {
            Assert.IsNull(GetCollaborationFactory().ForkFinder);
        }

        [Test]
        public void UnknownApiReturnsUnableToWorkOutPlatform()
        {
            var collaborationFactory = GetCollaborationFactory();

            var exception = Assert.Throws<NuKeeperException>(() => collaborationFactory.Initialise(new Uri("https://unknown.com/"), null));
            Assert.AreEqual(exception.Message, "Unable to work out platform for uri https://unknown.com/");
        }

        [Test]
        public void AzureDevOpsUrlReturnsAzureDevOps()
        {
            var collaborationFactory = GetCollaborationFactory();
            collaborationFactory.Initialise(new Uri("https://dev.azure.com"), "token");
            AssertAzureDevOps(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        [Test]
        public void GithubUrlReturnsGitHub()
        {
            var collaborationFactory = GetCollaborationFactory();
            collaborationFactory.Initialise(new Uri("https://api.github.com"), "token");
            AssertGithub(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        private static void AssertAreSameObject(ICollaborationFactory collaborationFactory)
        {
            var forkFinder = collaborationFactory.ForkFinder;
            Assert.AreSame(forkFinder, collaborationFactory.ForkFinder);

            var settings = collaborationFactory.Settings;
            Assert.AreSame(settings, collaborationFactory.Settings);
        }

        private static void AssertGithub(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<GitHubForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
        }

        private static void AssertAzureDevOps(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<AzureDevOpsForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
        }
    }
}
