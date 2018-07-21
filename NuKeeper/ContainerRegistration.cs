using NuKeeper.Engine;
using NuKeeper.Local;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init()
        {
            var container = new Container();

            Register(container);
            ContainerInspectionRegistration.Register(container);
            ContainerUpdateRegistration.Register(container);
            CreatorRegistrations.Initialize(container);

            return container;
        }

        private static void Register(Container container)
        {
            container.Register<GithubEngine>();
        }
    }
}
