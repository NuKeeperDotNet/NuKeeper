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

        private static Settings MakeValidSettings()
        {
            var settings = new Settings();
            settings.ModalSettings = new ModalSettings
                {
                    Mode = GithubMode.Organisation,
                    OrganisationName = "test1"
                };
            settings.GithubAuthSettings = new GithubAuthSettings(new Uri("http://foo.com/bar"), "abc123");
            settings.UserPreferences = new UserPreferences();
            return settings;
        }
    }
}
