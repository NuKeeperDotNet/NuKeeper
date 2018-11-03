using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Commands
{
    internal abstract class GitHubNuKeeperCommand : CommandBase
    {
        private readonly ICollaborationEngine _engine;
        protected readonly ISettingsReader SettingsReader;

        [Argument(1, Name = "Token",
            Description = "GitHub personal access token to authorise access to GitHub server.")]
        public string GitHubToken { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "f", LongName = "fork",
            Description = "Prefer to make branches on a fork of the writer repository, or on that repository itself. Allowed values are PreferFork, PreferSingleRepository, SingleRepositoryOnly. Defaults to PreferFork.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected ForkMode ForkMode { get; } = ForkMode.PreferFork;

        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "maxpr",
            Description = "The maximum number of pull requests to raise on any repository. Defaults to 3.")]
        public int? MaxPullRequestsPerRepository { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "co", LongName = "consolidate",
            Description = "Consolidate updates into a single pull request. Defaults to false.")]
        public bool? Consolidate { get; set; }

        [Option(CommandOptionType.MultipleValue, ShortName = "l", LongName = "label",
            Description = "Label to apply to GitHub pull requests. Defaults to 'nukeeper'. Multiple labels can be provided by specifying this option multiple times.")]
        public List<string> Label { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "g", LongName = "api",
            Description = "GitHub Api Base Url. If you are using an internal GitHub server and not the public one, you must set it to the api url for your GitHub server.")]
        public string GithubApiEndpoint { get; set; }
        
        protected GitHubNuKeeperCommand(ICollaborationEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache, ISettingsReader settingsReader) :
            base(logger, fileSettingsCache)
        {
            _engine = engine;
            SettingsReader = settingsReader;
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            var authSettings = SettingsReader.AuthSettings(GithubApiEndpoint, GitHubToken);

            if (authSettings.ApiBase == null)
            {
                return ValidationResult.Failure($"Bad Api base '{GithubApiEndpoint}'");
            }

            if (string.IsNullOrWhiteSpace(authSettings.Token))
            {
                return ValidationResult.Failure("The required access token was not found");
            }

            settings.AuthSettings = authSettings;


            var fileSettings = FileSettingsCache.GetSettings();
            settings.UserSettings.ConsolidateUpdatesInSinglePullRequest =
                Concat.FirstValue(Consolidate, fileSettings.Consolidate, false);


            const int defaultMaxPullRequests = 3;
            settings.PackageFilters.MaxPackageUpdates =
                Concat.FirstValue(MaxPullRequestsPerRepository, fileSettings.MaxPr, defaultMaxPullRequests);

            settings.UserSettings.ForkMode = ForkMode;

            var defaultLabels = new List<string> { "nukeeper" };

            settings.SourceControlServerSettings.Labels =
                Concat.FirstPopulatedList(Label, fileSettings.Label, defaultLabels);

            return ValidationResult.Success;
        }

        protected override async Task<int> Run(SettingsContainer settings)
        {
            await _engine.Run(settings);
            return 0;
        }
    }
}
