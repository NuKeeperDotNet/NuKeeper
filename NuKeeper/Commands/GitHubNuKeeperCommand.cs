using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    internal abstract class GitHubNuKeeperCommand : CommandBase
    {
        private readonly IGitHubEngine _engine;

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
        public string GithubApiEndpoint { get; set;  }

        protected GitHubNuKeeperCommand(IGitHubEngine engine, IConfigureLogger logger, IFileSettingsCache fileSettingsCache) :
            base(logger, fileSettingsCache)
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

            var apiBase = GithubEndpointWithFallback();

            if (string.IsNullOrWhiteSpace(apiBase))
            {
                return ValidationResult.Failure("No GitHub Api base found");
            }

            if (!Uri.TryCreate(apiBase, UriKind.Absolute, out var githubUri))
            {
                return ValidationResult.Failure($"Bad GitHub Api base '{GithubApiEndpoint}'");
            }

            var token = ReadToken();
            if (string.IsNullOrWhiteSpace(token))
            {
                return ValidationResult.Failure("The required GitHub access token was not found");
            }

            var githubUrl = GitSettingsReader.EnsureTrailingSlash(githubUri);

            var fileSettings = FileSettingsCache.GetSettings();

            settings.GithubAuthSettings = new AuthSettings(githubUrl, token);

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

        private string GithubEndpointWithFallback()
        {
            const string defaultGithubApi = "https://api.github.com/";
            var fileSetting = FileSettingsCache.GetSettings();
            return Concat.FirstValue(GithubApiEndpoint, fileSetting.Api, defaultGithubApi);
        }

        private string ReadToken()
        {
            if (!string.IsNullOrWhiteSpace(GitHubToken))
            {
                return GitHubToken;
            }

            var envToken = Environment.GetEnvironmentVariable("NuKeeper_github_token");
            if (!string.IsNullOrWhiteSpace(envToken))
            {
                return envToken;
            }

            return string.Empty;
        }
    }
}
