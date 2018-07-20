using System;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Commands;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper
{
    internal abstract class GitHubNuKeeperCommand : CommandBase
    {
        [Argument(1, Name = "Token",
            Description =
                "GitHub personal access token to authorise access to GitHub server.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        protected string GitHubToken { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "mr", LongName = "maxrepo",
            Description = "The maximum number of repositories to change. Defaults to 10.")]
        protected int AllowedMaxRepositoriesChangedChange { get; } = 10;

        [Option(CommandOptionType.SingleValue, ShortName = "f", LongName = "fork",
            Description = "Prefer to make branches on a fork of the target repository, or on that repository itself. Allowed values are PreferFork, PreferSingleRepository, SingleRepositoryOnly. Defaults to PreferFork.")]
        protected ForkMode ForkMode { get; } = ForkMode.PreferFork;

        [Option(CommandOptionType.SingleValue, ShortName = "mp", LongName = "maxpr",
            Description = "The maximum number of pull requests to raise on any repository. Defaults to 3.")]
        protected int MaxPullRequestsPerRepository { get; } = 3;

        protected GitHubNuKeeperCommand(IConfigureLogLevel logger) : base(logger)
        {
        }

        protected override Task<int> Run(SettingsContainer settings)
        {
            settings.GithubAuthSettings = new GithubAuthSettings(new Uri("https://api.github.com"), GitHubToken);
            settings.UserSettings.MaxRepositoriesChanged = AllowedMaxRepositoriesChangedChange;
            settings.UserSettings.MaxPullRequestsPerRepository = MaxPullRequestsPerRepository;
            settings.UserSettings.ForkMode = ForkMode;
            return Task.FromResult(0);
        }
    }
}
