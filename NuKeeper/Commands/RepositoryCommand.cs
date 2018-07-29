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
        private readonly GithubEngine _engine;

        [Argument(0, Name = "GitHub repository URI", Description = "The URI of the repository to scan.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GitHubRepositoryUri { get; }

        public RepositoryCommand(GithubEngine engine, IConfigureLogLevel logger) : base(logger)
        {
            _engine = engine;
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

            settings.ModalSettings.Mode = RunMode.Repository;
            settings.ModalSettings.Repository = GitSettingsReader.ReadRepositorySettings(repoUri);

            if (settings.ModalSettings.Repository == null)
            {
                return ValidationResult.Failure($"Cound not read GitHub repository URI: '{GitHubRepositoryUri}'");
            }

            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(settings);
            return 0;
        }
    }
}
