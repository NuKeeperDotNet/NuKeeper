using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Sources;

namespace NuKeeper.Commands
{
    [HelpOption]
    internal abstract class CommandBase
    {
        [Option(CommandOptionType.SingleValue, ShortName = "c", LongName = "change",
            Description = "Allowed version change: None, Patch, Minor, Major. Defaults to Major.")]
        protected VersionChange AllowedChange { get; } = VersionChange.Major;

        [Option(CommandOptionType.MultipleValue, ShortName = "s", LongName = "source",
            Description =
                "Specifies a NuGet package source to use during the operation. This setting overrides all of the sources specified in the NuGet.config files. Multiple sources can be provided by specifying this option multiple times.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string[] Source { get; }

        protected NuGetSources NuGetSources => Source == null?  null : new NuGetSources(Source);
    }
}
