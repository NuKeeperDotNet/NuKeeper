using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    internal abstract class LocalNuKeeperCommand : CommandBase
    {
        [Argument(0, Description = "The path to a .sln or .csproj file, or to a directory containing a .NET Core solution/project. " +
                                   "If none is specified, the current directory will be used.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string Path { get; }

        protected LocalNuKeeperCommand(IConfigureLogLevel logger) : base(logger)
        {
        }

        protected override Task<int> Run(SettingsContainer settings)
        {
            settings.UserSettings.Directory = Path;
            return Task.FromResult(0);
        }
    }
}
