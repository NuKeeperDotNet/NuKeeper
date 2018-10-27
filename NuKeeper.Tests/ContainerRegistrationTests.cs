using System;
using NuKeeper.Abstract.Local;
using NuKeeper.Commands;
using NuKeeper.Github.Engine;
using NUnit.Framework;

namespace NuKeeper.Tests
{
    [TestFixture]
    public class ContainerRegistrationTests
    {
        [Test]
        public void RootCanBeResolved()
        {
            var container = ContainerRegistration.Init();

            var engine = container.GetInstance<GitHubEngine>();

            Assert.That(engine, Is.Not.Null);
        }

        [Test]
        public void InspectorCanBeResolved()
        {
            var container = ContainerRegistration.Init();

            var inspector = container.GetInstance<LocalEngine>();

            Assert.That(inspector, Is.Not.Null);
        }

        [TestCase(typeof(InspectCommand))]
        [TestCase(typeof(UpdateCommand))]
        [TestCase(typeof(RepositoryCommand))]
        [TestCase(typeof(OrganisationCommand))]
        [TestCase(typeof(GlobalCommand))]
        public void CommandsCanBeResolved(Type commandType)
        {
            var container = ContainerRegistration.Init();

            var command = container.GetInstance(commandType);

            Assert.That(command, Is.Not.Null);
        }
    }
}
