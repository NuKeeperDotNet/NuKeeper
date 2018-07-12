using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Commands
{
    [HelpOption]
    internal abstract class CommandBase
    {
        private readonly IReconfigurableLogger _logger;

        [Option(CommandOptionType.SingleValue, ShortName = "c", LongName = "change",
            Description = "Allowed version change: Patch, Minor, Major. Defaults to Major.")]
        protected VersionChange AllowedChange { get; } = VersionChange.Major;

        [Option(CommandOptionType.MultipleValue, ShortName = "s", LongName = "source",
            Description =
                "Specifies a NuGet package source to use during the operation. This setting overrides all of the sources specified in the NuGet.config files. Multiple sources can be provided by specifying this option multiple times.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string[] Source { get; }

        protected NuGetSources NuGetSources => Source == null?  null : new NuGetSources(Source);

        [Option(CommandOptionType.SingleValue, ShortName = "v", LongName = "verbosity", Description = "Sets the verbosity level of the command. Allowed values are quiet, minimal, normal, detailed.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected LogLevel Verbosity { get; } = LogLevel.Normal;

        protected CommandBase(IReconfigurableLogger logger)
        {
            _logger = logger;
        }

        // ReSharper disable once UnusedMember.Global
        public async Task<int> OnExecute()
        {
            _logger.SetLogLevel(Verbosity);
            return await Run();
        }

        protected abstract Task<int> Run();
    }
}
