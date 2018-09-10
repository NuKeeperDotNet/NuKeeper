using System;
using System.Threading.Tasks;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Engine;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.Report;

namespace NuKeeper.Commands
{
    internal abstract class GitHubNuKeeperCommand : CommandBase
    {
        private readonly GitHubEngine _engine;

        [Argument(1, Name = "Token",
            Description =
                "GitHub personal access token to authorise access to GitHub server.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GitHubToken { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "x", LongName = "maxrepo",
            Description = "The maximum number of repositories to change. Defaults to 10.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected int AllowedMaxRepositoriesChangedChange { get; } = 10;

        [Option(CommandOptionType.SingleValue, ShortName = "f", LongName = "fork",
            Description =
                "Prefer to make branches on a fork of the target repository, or on that repository itself. Allowed values are PreferFork, PreferSingleRepository, SingleRepositoryOnly. Defaults to PreferFork.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected ForkMode ForkMode { get; } = ForkMode.PreferFork;

        [Option(CommandOptionType.SingleValue, ShortName = "p", LongName = "maxpr",
            Description = "The maximum number of pull requests to raise on any repository. Defaults to 3.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected int MaxPullRequestsPerRepository { get; } = 3;

        [Option(CommandOptionType.NoValue, ShortName = "co", LongName = "consolidate",
            Description = "Consolidate updates into a single pull request. Defaults to false.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected bool ConsolidateUpdatesInSinglePullRequest { get; } = false;

        [Option(CommandOptionType.MultipleValue, ShortName = "l", LongName = "label",
            Description =
                "Label to apply to GitHub pull requests. Defaults to 'nukeeper'. Multiple labels can be provided by specifying this option multiple times.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string[] Label { get; } = { "nukeeper" };

        [Option(CommandOptionType.SingleValue, ShortName = "g", LongName = "api",
            Description =
                "GitHub Api Base Url. If you are using an internal GitHub server and not the public one, you must set it to the api url for your GitHub server.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GithubApiEndpoint { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "r", LongName = "report",
            Description =
                "Controls if a CSV report file of possible updates is generated. Allowed values are Off, On, ReportOnly (which skips applying updates). Defaults to Off.")]
        protected ReportMode ReportMode { get; } = ReportMode.Off;

        protected GitHubNuKeeperCommand(GitHubEngine engine, IConfigureLogLevel logger, IFileSettingsCache fileSettingsCache) :
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

            settings.GithubAuthSettings = new GithubAuthSettings(githubUrl, token);

            settings.UserSettings.MaxRepositoriesChanged = AllowedMaxRepositoriesChangedChange;
            settings.UserSettings.ConsolidateUpdatesInSinglePullRequest = ConsolidateUpdatesInSinglePullRequest;
            settings.PackageFilters.MaxPackageUpdates = MaxPullRequestsPerRepository;
            settings.UserSettings.ForkMode = ForkMode;
            settings.UserSettings.ReportMode = ReportMode;

            var fileSetting = FileSettingsCache.Get();

            settings.SourceControlServerSettings.Labels =
                Concat.AllPopulated(Label, fileSetting.Label).ToList();

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
            var fileSetting = FileSettingsCache.Get();
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
