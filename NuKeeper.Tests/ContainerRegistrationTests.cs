using NUnit.Framework;
using NuKeeper.Engine;
using NuKeeper.Local;

namespace NuKeeper.Tests
{
    [TestFixture]
    public class ContainerRegistrationTests
    {
        [Test]
        public void RootCanBeResolved()
        {
            var container = ContainerRegistration.Init();

            var engine = container.GetInstance<GithubEngine>();

            Assert.That(engine, Is.Not.Null);
        }

        [Test]
        public void InspectorCanBeResolved()
        {
            var container = ContainerRegistration.Init();

            var inspector = container.GetInstance<LocalEngine>();

            Assert.That(inspector, Is.Not.Null);
        }
    }
}
