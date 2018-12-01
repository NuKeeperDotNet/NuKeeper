using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Inspection.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NuKeeper.Collaboration;

namespace NuKeeper.Commands
{
    internal abstract class CollaborationPlatformCommand : CommandBase
    {
        private readonly ICollaborationEngine _engine;
        public readonly ICollaborationFactory CollaborationFactory;

        [Argument(1, Name = "Token",
            Description = "Personal access token to authorise access to server.")]
        public string PersonalAccessToken { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "f", LongName = "fork",
            Description =
                "Prefer to make branches on a fork of the writer repository, or on that repository itself. Allowed values are PreferFork, PreferSingleRepository, SingleRepositoryOnly.")]
        public ForkMode? ForkMode { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "maxpr",
            Description = "The maximum number of pull requests to raise on any repository. Defaults to 3.")]
        public int? MaxPullRequestsPerRepository { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "co", LongName = "consolidate",
            Description = "Consolidate updates into a single pull request. Defaults to false.")]
        public bool? Consolidate { get; set; }

        [Option(CommandOptionType.MultipleValue, ShortName = "l", LongName = "label",
            Description =
                "Label to apply to GitHub pull requests. Defaults to 'nukeeper'. Multiple labels can be provided by specifying this option multiple times.")]
        public List<string> Label { get; set; }

        [Option(CommandOptionType.SingleValue, ShortName = "g", LongName = "api",
            Description =
                "Api Base Url. If you are using an internal server and not a public one, you must set it to the api url of your server.")]
        public string ApiEndpoint { get; set; }

        protected CollaborationPlatformCommand(ICollaborationEngine engine, IConfigureLogger logger,
            IFileSettingsCache fileSettingsCache, ICollaborationFactory collaborationFactory) :
            base(logger, fileSettingsCache)
        {
            _engine = engine;
            CollaborationFactory = collaborationFactory;
        }

        protected override ValidationResult PopulateSettings(SettingsContainer settings)
        {
            var baseResult = base.PopulateSettings(settings);
            if (!baseResult.IsSuccess)
            {
                return baseResult;
            }

            var fileSettings = FileSettingsCache.GetSettings();

            var endpoint = Concat.FirstValue(ApiEndpoint, fileSettings.Api, settings.SourceControlServerSettings.Repository?.ApiUri.ToString()); 

            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri))
            {
                return ValidationResult.Failure($"Bad Api Base '{endpoint}'");
            }


            var initResult =  CollaborationFactory.Initialise(baseUri, PersonalAccessToken, ForkMode);

            if (!initResult.IsSuccess)
            {
                return initResult;
            }

            if (CollaborationFactory.Settings.Token == null)
            {
                return ValidationResult.Failure("The required access token was not found");
            }

            settings.UserSettings.ConsolidateUpdatesInSinglePullRequest =
                Concat.FirstValue(Consolidate, fileSettings.Consolidate, false);

            const int defaultMaxPullRequests = 3;
            settings.PackageFilters.MaxPackageUpdates =
                Concat.FirstValue(MaxPullRequestsPerRepository, fileSettings.MaxPr, defaultMaxPullRequests);

            var defaultLabels = new List<string> {"nukeeper"};

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
