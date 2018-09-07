using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for a single repository on GitHub.")]
    internal class RepositoryCommand : GitHubNuKeeperCommand
    {
        [Argument(0, Name = "GitHub repository URI", Description = "The URI of the repository to scan.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GitHubRepositoryUri { get; }

        public RepositoryCommand(GitHubEngine engine, IConfigureLogLevel logger) : base(engine, logger)
        {
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            Uri repoUri;
            if (!Uri.TryCreate(GitHubRepositoryUri, UriKind.Absolute, out repoUri))
            {
                return ValidationResult.Failure($"Bad GitHub repository URI: '{GitHubRepositoryUri}'");
            }

            settings.SourceControlServerSettings.Repository = GitSettingsReader.ReadRepositorySettings(repoUri);

            if (settings.SourceControlServerSettings.Repository == null)
            {
                return ValidationResult.Failure($"Cound not read GitHub repository URI: '{GitHubRepositoryUri}'");
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Repository;
            return ValidationResult.Success;
        }
    }
}
