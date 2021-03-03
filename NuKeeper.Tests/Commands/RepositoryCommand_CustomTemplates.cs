using NSubstitute;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.AzureDevOps;
using NuKeeper.Collaboration;
using NuKeeper.Commands;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Commands
{
    public class RepositoryCommand_CustomTemplates
    {
        private IFileSettingsCache _fileSettings;
        private ICollaborationFactory _collaborationFactory;

        [SetUp]
        public void Initialize()
        {
            _fileSettings = Substitute.For<IFileSettingsCache>();
            _collaborationFactory = Substitute.For<ICollaborationFactory>();
            _fileSettings
                .GetSettings()
                .Returns(FileSettings.Empty());
            _collaborationFactory
                .Initialise(
                    Arg.Any<Uri>(),
                    Arg.Any<string>(),
                    Arg.Any<ForkMode?>(),
                    Arg.Any<Platform?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<Dictionary<string, object>>()
                )
                .Returns(ValidationResult.Success);
            _collaborationFactory
                .Settings
                .Returns(new CollaborationPlatformSettings { Token = "mytoken" });
        }

        [Test]
        public async Task OnExecute_CustomTemplatesFromFile_CallsInitialiseCollaborationFactoryWithCustomTemplates()
        {
            var commitTemplate = "commit template";
            var prTitleTemplate = "pr title template";
            var prBodyTemplate = "pr body template";
            _fileSettings
                .GetSettings()
                .Returns(
                    new FileSettings
                    {
                        CommitMessageTemplate = commitTemplate,
                        PullRequestTitleTemplate = prTitleTemplate,
                        PullRequestBodyTemplate = prBodyTemplate
                    }
                );
            var command = MakeCommand();

            await command.OnExecute();

            await _collaborationFactory
                .Received()
                .Initialise(
                    Arg.Any<Uri>(),
                    Arg.Any<string>(),
                    Arg.Any<ForkMode?>(),
                    Arg.Any<Platform?>(),
                    commitTemplate,
                    prTitleTemplate,
                    prBodyTemplate,
                    Arg.Any<IDictionary<string, object>>()
                );
        }

        [Test]
        public async Task OnExecute_CustomTemplatesFromCli_CallsInitialiseCollaborationFactoryWithCustomTemplates()
        {
            var commitTemplate = "commit template";
            var prTitleTemplate = "pr title template";
            var prBodyTemplate = "pr body template";
            var command = MakeCommand();
            command.CommitMessageTemplate = commitTemplate;
            command.PullRequestTitleTemplate = prTitleTemplate;
            command.PullRequestBodyTemplate = prBodyTemplate;

            await command.OnExecute();

            await _collaborationFactory
                .Received()
                .Initialise(
                    Arg.Any<Uri>(),
                    Arg.Any<string>(),
                    Arg.Any<ForkMode?>(),
                    Arg.Any<Platform?>(),
                    commitTemplate,
                    prTitleTemplate,
                    prBodyTemplate,
                    Arg.Any<IDictionary<string, object>>()
                );
        }

        [Test]
        public async Task OnExecute_CustomTemplatesFromCliAndFile_CallsInitialiseCollaborationFactoryWithCustomTemplatesFromCli()
        {
            var commitTemplate = "commit template";
            var prTitleTemplate = "pr title template";
            var prBodyTemplate = "pr body template";
            _fileSettings
                .GetSettings()
                .Returns(
                    new FileSettings
                    {
                        CommitMessageTemplate = "commit template from file",
                        PullRequestTitleTemplate = "pr title template from file",
                        PullRequestBodyTemplate = "pr body template from file"
                    }
                );
            var command = MakeCommand();
            command.CommitMessageTemplate = commitTemplate;
            command.PullRequestTitleTemplate = prTitleTemplate;
            command.PullRequestBodyTemplate = prBodyTemplate;

            await command.OnExecute();

            await _collaborationFactory
                .Received()
                .Initialise(
                    Arg.Any<Uri>(),
                    Arg.Any<string>(),
                    Arg.Any<ForkMode?>(),
                    Arg.Any<Platform?>(),
                    commitTemplate,
                    prTitleTemplate,
                    prBodyTemplate,
                    Arg.Any<IDictionary<string, object>>()
                );
        }

        private RepositoryCommand MakeCommand()
        {
            var engine = Substitute.For<ICollaborationEngine>();
            var logger = Substitute.For<IConfigureLogger>();
            var settingReader = new TfsSettingsReader(new MockedGitDiscoveryDriver(), Substitute.For<IEnvironmentVariablesProvider>());
            var settingsReaders = new List<ISettingsReader> { settingReader };

            return new RepositoryCommand(
                engine,
                logger,
                _fileSettings,
                _collaborationFactory,
                settingsReaders
            )
            {
                RepositoryUri = "http://tfs.myorganization.com/tfs/DefaultCollection/MyProject/_git/MyRepository",
                PersonalAccessToken = "mytoken"
            };
        }
    }
}
