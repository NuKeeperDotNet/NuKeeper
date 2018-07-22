using System;
using McMaster.Extensions.CommandLineUtils;
using NuKeeper.Configuration;
using NuKeeper.Inspection.Logging;

namespace NuKeeper.Commands
{
    internal abstract class GitHubNuKeeperCommand : CommandBase
    {
        [Argument(1, Name = "Token",
            Description =
                "GitHub personal access token to authorise access to GitHub server.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string GitHubToken { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "mr", LongName = "maxrepo",
            Description = "The maximum number of repositories to change. Defaults to 10.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected int AllowedMaxRepositoriesChangedChange { get; } = 10;

        [Option(CommandOptionType.SingleValue, ShortName = "f", LongName = "fork",
            Description = "Prefer to make branches on a fork of the target repository, or on that repository itself. Allowed values are PreferFork, PreferSingleRepository, SingleRepositoryOnly. Defaults to PreferFork.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected ForkMode ForkMode { get; } = ForkMode.PreferFork;

        [Option(CommandOptionType.SingleValue, ShortName = "mp", LongName = "maxpr",
            Description = "The maximum number of pull requests to raise on any repository. Defaults to 3.")]
        // ReSharper disable once MemberCanBePrivate.Global
        protected int MaxPullRequestsPerRepository { get; } = 3;

        [Option(CommandOptionType.MultipleValue, ShortName = "l", LongName = "label",
            Description =
                "Label to apply to GitHub pull requests. Defaults to 'nukeeper'. Multiple labels can be provided by specifying this option multiple times.")]
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        // ReSharper disable once MemberCanBePrivate.Global
        protected string[] Label { get; } = {"nukeeper"};

        protected GitHubNuKeeperCommand(IConfigureLogLevel logger) : base(logger)
        {
        }

        protected override void PopulateSettings(SettingsContainer settings)
        {
            settings.GithubAuthSettings = new GithubAuthSettings(
                new Uri("https://api.github.com"),
                GitHubToken);

            settings.UserSettings.MaxRepositoriesChanged = AllowedMaxRepositoriesChangedChange;
            settings.UserSettings.MaxPullRequestsPerRepository = MaxPullRequestsPerRepository;
            settings.UserSettings.ForkMode = ForkMode;
            settings.ModalSettings.Labels = Label;
        }
    }
}
