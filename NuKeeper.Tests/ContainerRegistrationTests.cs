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
            var org = new OrganisationModeSettings
            {
                GithubApiBase = new Uri("https://github.com/NuKeeperDotNet"),
                GithubToken = "abc123"
            };
            return new Settings(org);
        }
    }
}
