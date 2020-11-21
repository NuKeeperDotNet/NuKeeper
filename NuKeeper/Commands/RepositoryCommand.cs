using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using McMaster.Extensions.CommandLineUtils;

using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Collaboration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command("repo", "r", "repository", Description = "Performs version checks and generates pull requests for a single repository.")]
    internal class RepositoryCommand : CollaborationPlatformCommand
    {
        [Option(CommandOptionType.NoValue, ShortName = "", LongName = "setautomerge",
            Description = "Set automatically auto merge for created pull request. Works only for Azure Devops. Defaults to false.")]
        public bool? SetAutoMerge { get; set; }

        private readonly IEnumerable<ISettingsReader> _settingsReaders;

        public RepositoryCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory, IEnumerable<ISettingsReader> settingsReaders)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
            _settingsReaders = settingsReaders;
        }

        [Argument(0, Name = "Repository URI", Description = "The URI of the repository to scan.")]
        public string RepositoryUri { get; set; }

        [Option(CommandOptionType.SingleValue, LongName = "targetBranch",
            Description = "If the target branch is another branch than that you are currently on, set this to the target")]
        public string TargetBranch { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "cdir", Description = "If you want NuKeeper to check out the repository to an alternate path, set it here (by default, a temporary directory is used).")]
        protected string CheckoutDirectory { get; set; }

        protected override async Task<ValidationResult> PopulateSettings(SettingsContainer settings)
        {
            if (string.IsNullOrWhiteSpace(RepositoryUri))
            {
                return ValidationResult.Failure($"Missing repository URI");
            }

            Uri repoUri;

            try
            {
                repoUri = RepositoryUri.ToUri();
            }
            catch (UriFormatException)
            {
                return ValidationResult.Failure($"Bad repository URI: '{RepositoryUri}'");
            }

            ISettingsReader reader = await TryGetSettingsReader(repoUri, Platform);

            if (reader == null)
            {
                return ValidationResult.Failure($"Unable to work out which platform to use {RepositoryUri} could not be matched");
            }

            settings.SourceControlServerSettings.Repository = await reader.RepositorySettings(repoUri, SetAutoMerge ?? false, TargetBranch);

            var baseResult = await base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            if (settings.SourceControlServerSettings.Repository == null)
            {
                return ValidationResult.Failure($"Could not read repository URI: '{RepositoryUri}'");
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Repository;
            settings.UserSettings.MaxRepositoriesChanged = 1;
            settings.UserSettings.Directory = CheckoutDirectory;

            return ValidationResult.Success;
        }

        private async Task<ISettingsReader> TryGetSettingsReader(Uri repoUri, Platform? platform)
        {
            // If the platform was specified explicitly, get the reader by platform.
            if (platform.HasValue)
            {
                return _settingsReaders.Single(s => s.Platform == Platform);
            }

            // Otherwise, use the Uri to guess which platform to use.
            foreach (var reader in _settingsReaders)
            {
                if (await reader.CanRead(repoUri))
                {
                    return reader;
                }
            }

            return null;
        }
    }
}
