using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Inspection.Logging;
using System;
using System.Collections.Generic;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Collaboration;

namespace NuKeeper.Commands
{
    [Command("repo", "r", "repository", Description = "Performs version checks and generates pull requests for a single repository.")]
    internal class RepositoryCommand : CollaborationPlatformCommand
    {
        [Argument(0, Name = "Repository URI", Description = "The URI of the repository to scan.")]
        public string RepositoryUri { get; set; }

        [Option(CommandOptionType.SingleValue, LongName = "targetBranch",
            Description = "If the target branch is another branch than that you are currently on, set this to the target")]
        public string TargetBranch { get; set; }

        private readonly IEnumerable<ISettingsReader> _settingsReaders;

        public RepositoryCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory, IEnumerable<ISettingsReader> settingsReaders)
            : base(engine, logger, fileSettingsCache, collaborationFactory)
        {
            _settingsReaders = settingsReaders;
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            Uri repoUri;

            try
            {
                repoUri = RepositoryUri.ToUri();
            }
            catch (UriFormatException)
            {
                return ValidationResult.Failure($"Bad repository URI: '{RepositoryUri}'");
            }

            var didRead = false;
            foreach (var reader in _settingsReaders)
            {
                if (reader.CanRead(repoUri))
                {
                    didRead = true;
                    settings.SourceControlServerSettings.Repository = reader.RepositorySettings(repoUri, TargetBranch);
                }
            }

            if (!didRead)
            {
                return ValidationResult.Failure($"Unable to work out which platform to use {RepositoryUri} could not be matched");
            }

            var baseResult = base.PopulateSettings(settings);
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
            return ValidationResult.Success;
        }
    }
}
