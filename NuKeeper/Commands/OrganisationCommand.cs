using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    [Command(Description = "Performs version checks and generates pull requests for all repositories in a github organisation.")]
    internal class OrganisationCommand : MultipleRepositoryCommand
    {
        [Argument(0, Name = "GitHub organisation name", Description = "The organisation to scan.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GithubOrganisationName { get; }

        public OrganisationCommand(GitHubEngine engine, IConfigureLogLevel logger) : base(engine, logger)
        {
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            settings.SourceControlServerSettings.Scope = ServerScope.Organisation;
            settings.SourceControlServerSettings.OrganisationName = GithubOrganisationName;
            return ValidationResult.Success;
        }
    }
}
