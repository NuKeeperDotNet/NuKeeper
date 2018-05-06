using System;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NUnit.Framework;

namespace NuKeeper.Tests
{
    [TestFixture]
    public class ContainerRegistrationTests
    {
        [Test]
        public void RootCanBeResolved()
        {
            var container = ContainerRegistration.Init(MakeValidSettings());

            var engine = container.GetInstance<GithubEngine>();

            Assert.That(engine, Is.Not.Null);
        }

        [Test]
        public void InspectorCanBeResolved()
        {
            var container = ContainerRegistration.Init(MakeValidSettings());

            var inspector = container.GetInstance<Inspector>();

            Assert.That(inspector, Is.Not.Null);
        }

        private static SettingsContainer MakeValidSettings()
        {
            var settings = new SettingsContainer();
            settings.ModalSettings = new ModalSettings
                {
                    Mode = GithubMode.Organisation,
                    OrganisationName = "test1"
                };
            settings.GithubAuthSettings = new GithubAuthSettings(new Uri("http://foo.com/bar"), "abc123");
            settings.UserSettings = new UserSettings();
            return settings;
        }
    }
}
