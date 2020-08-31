using System;
using NuKeeper.Collaboration;
using NuKeeper.Commands;
using NUnit.Framework;
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

            var engine = container.GetInstance<ICollaborationEngine>();

            Assert.That(engine, Is.Not.Null);
            Assert.That(engine, Is.TypeOf<CollaborationEngine>());
        }

        [Test]
        public void InspectorCanBeResolved()
        {
            var container = ContainerRegistration.Init();

            var inspector = container.GetInstance<ILocalEngine>();

            Assert.That(inspector, Is.Not.Null);
            Assert.That(inspector, Is.TypeOf<LocalEngine>());
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
