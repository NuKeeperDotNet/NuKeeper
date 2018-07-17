using NuKeeper.Github;
using SimpleInjector;

namespace NuKeeper
{
    public static class CreatorRegistrations
    {
        public static void Initialize(Container container)
        {
            container.Register<ICreate<IGithub>, OctokitClientCreator>();
        }
    }
}
