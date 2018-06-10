using NuKeeper.Configuration;
using NuKeeper.Update;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using NuKeeper.Update.Selection;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerUpdateRegistration
    {
        public static void Register(Container container, SettingsContainer settings)
        {
            container.Register(() => settings.UserSettings.NuGetSources, Lifestyle.Singleton);
            container.Register(() => MakeFilterSettings(settings.UserSettings), Lifestyle.Singleton);

            container.Register<IUpdateSelection, UpdateSelection>();
            container.Register<IFileRestoreCommand, NuGetFileRestoreCommand>();
            container.Register<IExternalProcess, ExternalProcess>();
            container.Register<IUpdateRunner, UpdateRunner>();
        }

        private static FilterSettings MakeFilterSettings(UserSettings settings)
        {
            return new FilterSettings
            {
                Excludes = settings.PackageExcludes,
                Includes = settings.PackageIncludes,
                MaxPullRequests = settings.MaxPullRequestsPerRepository,
                MinimumAge = settings.MinimumPackageAge
            };
        }
    }
}
