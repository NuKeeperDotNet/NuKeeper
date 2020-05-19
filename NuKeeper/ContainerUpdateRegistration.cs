using NuKeeper.Update;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using SimpleInjector;

namespace NuKeeper
{
    internal static class ContainerUpdateRegistration
    {
        internal static void Register(Container container)
        {
            container.Register<IFileRestoreCommand, NuGetFileRestoreCommand>();
            container.Register<INuGetUpdatePackageCommand, NuGetUpdatePackageCommand>();
            container.Register<IDotNetUpdatePackageCommand, DotNetUpdatePackageCommand>();
            container.Register<IUpdateProjectImportsCommand, UpdateProjectImportsCommand>();
            container.Register<IUpdateNuspecCommand, UpdateNuspecCommand>();
            container.Register<IUpdateDirectoryBuildTargetsCommand, UpdateDirectoryBuildTargetsCommand>();
            container.Register<IExternalProcess, ExternalProcess>();
            container.Register<IMonoExecutor, MonoExecutor>();
            container.Register<IUpdateRunner, UpdateRunner>();
            container.Register<INuGetPath, NuGetPath>();
        }
    }
}
