using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Engine.Packages;
using NuKeeper.Local;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerRegistration
    {
        public static Container Init(SettingsContainer settings)
        {
            var container = new Container();

            Register(container, settings);
            ContainerInspectionRegistration.Register(container);
            ContainerUpdateRegistration.Register(container, settings);
            CreatorRegistrations.Initialize(container);

            return container;
        }

        private static void Register(Container container, SettingsContainer settings)
        {
            container.Register(() => settings.ModalSettings, Lifestyle.Singleton);
            container.Register(() => settings.GithubAuthSettings, Lifestyle.Singleton);
            container.Register(() => settings.UserSettings, Lifestyle.Singleton);

            container.Register<GithubEngine>();

            container.Register<ILocalUpdater, LocalUpdater>();
        }
    }
}
