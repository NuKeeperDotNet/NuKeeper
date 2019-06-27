using NuKeeper.Update;
using NuKeeper.Update.Process;
using NuKeeper.Update.ProcessRunner;
using SimpleInjector;

namespace NuKeeper
{
    public static class ContainerUpdateRegistration
    {
        public static void Register(Container container)
        {
            container.Register<IFileRestoreCommand, NuGetFileRestoreCommand>();
            container.Register<INuGetUpdatePackageCommand, NuGetUpdatePackageCommand>();
            container.Register<IDotNetUpdatePackageCommand, DotNetUpdatePackageCommand>();
            container.Register<IDotNetRestoreCommand, DotNetRestoreCommand>();
            container.Register<IUpdateProjectImportsCommand, UpdateProjectImportsCommand>();
            container.Register<IUpdateNuspecCommand, UpdateNuspecCommand>();
            container.Register<IUpdateDirectoryBuildTargetsCommand, UpdateDirectoryBuildTargetsCommand>();
            container.Register<IExternalProcess, ExternalProcess>();
            container.Register<IUpdateRunner, UpdateRunner>();
            container.Register<INuGetPath, NuGetPath>();
        }
    }
}
