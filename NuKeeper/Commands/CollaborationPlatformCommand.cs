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

        [Option(CommandOptionType.SingleValue, ShortName = "m", LongName = "maxpackageupdates",
            Description = "The maximum number of package updates to apply on one repository. Defaults to 3.")]
        public int? MaxPackageUpdates { get; set; }

        [Option(CommandOptionType.NoValue, ShortName = "n", LongName = "consolidate",
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

        [Option(CommandOptionType.SingleValue, ShortName = "", LongName = "platform",
            Description = "Sets the collaboration platform type. By default this is inferred from the Url.")]
        public Platform? Platform { get; set; }

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
            var forkMode = ForkMode ?? fileSettings.ForkMode;
            var platform = Platform ?? fileSettings.Platform;

            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var baseUri))
            {
                return ValidationResult.Failure($"Bad Api Base '{endpoint}'");
            }

            try
            {
                var collaborationResult = CollaborationFactory.Initialise(
                    baseUri, PersonalAccessToken,
                    forkMode, platform);

                if (!collaborationResult.IsSuccess)
                {
                    return collaborationResult;
                }
            }
            catch (Exception ex)
            {
                return ValidationResult.Failure(ex.Message);
            }

            if (CollaborationFactory.Settings.Token == null)
            {
                return ValidationResult.Failure("The required access token was not found");
            }

            settings.UserSettings.ConsolidateUpdatesInSinglePullRequest =
                Concat.FirstValue(Consolidate, fileSettings.Consolidate, false);

            const int defaultMaxPackageUpdates = 3;
            settings.PackageFilters.MaxPackageUpdates =
                Concat.FirstValue(MaxPackageUpdates, fileSettings.MaxPackageUpdates, defaultMaxPackageUpdates);

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
