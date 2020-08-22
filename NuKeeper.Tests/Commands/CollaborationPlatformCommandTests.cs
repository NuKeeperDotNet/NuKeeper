using NSubstitute;
using NUnit.Framework;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Collaboration;
using NuKeeper.Commands;
using NuKeeper.Inspection.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace NuKeeper.Tests.Commands
{
    [TestFixture]
    public class CollaborationPlatformCommandTests
    {
        private ICollaborationEngine _engine;
        private IConfigureLogger _logger;
        private IFileSettingsCache _fileSettingsCache;
        private ICollaborationFactory _collaborationFactory;

        [SetUp]
        public void Initialize()
        {
            _engine = Substitute.For<ICollaborationEngine>();
            _logger = Substitute.For<IConfigureLogger>();
            _fileSettingsCache = Substitute.For<IFileSettingsCache>();
            _collaborationFactory = Substitute.For<ICollaborationFactory>();

            _fileSettingsCache
                .GetSettings()
                .Returns(FileSettings.Empty());
            _collaborationFactory
                .Initialise(Arg.Any<Uri>(), Arg.Any<string>(), Arg.Any<ForkMode?>(), Arg.Any<Platform?>())
                .Returns(ValidationResult.Success);
        }


        [Test]
        public async Task OnExecute_ReviewersProvidedFromCli_CorrectlyPopulatesSettingsContainerWithReviewers()
        {
            var command = MakeCommand();
            _collaborationFactory
                .Settings
                .Returns(new CollaborationPlatformSettings { Token = command.PersonalAccessToken });
            command.Reviewers = new List<string> { "nukeeper@nukeeper.nukeeper" };

            await command.OnExecute();

            await _engine
                .Received()
                .Run(
                    Arg.Is<SettingsContainer>(s =>
                        s.SourceControlServerSettings.Reviewers.Contains("nukeeper@nukeeper.nukeeper")
                    )
                );
        }

        [Test]
        public async Task OnExecute_ReviewersProvidedFromFile_CorrectlyPopulatesSettingsContainerWithReviewers()
        {
            var command = MakeCommand();
            _fileSettingsCache
                .GetSettings()
                .Returns(new FileSettings { Reviewers = new List<string> { "nukeeper@nukeeper.nukeeper" } });
            _collaborationFactory
                .Settings
                .Returns(new CollaborationPlatformSettings { Token = command.PersonalAccessToken });

            await command.OnExecute();

            await _engine
                .Received()
                .Run(
                    Arg.Is<SettingsContainer>(s =>
                        s.SourceControlServerSettings.Reviewers.Contains("nukeeper@nukeeper.nukeeper")
                    )
                );
        }

        [Test]
        public async Task OnExecute_ReviewersProvidedFromCliAndFile_CorrectlyPopulatesSettingsContainerWithReviewersFromCli()
        {
            var command = MakeCommand();
            command.Reviewers = new List<string> { "notnukeeper@nukeeper.nukeeper" };
            _collaborationFactory
                .Settings
                .Returns(new CollaborationPlatformSettings { Token = command.PersonalAccessToken });
            _fileSettingsCache
                .GetSettings()
                .Returns(new FileSettings { Reviewers = new List<string> { "nukeeper@nukeeper.nukeeper" } });

            await command.OnExecute();

            await _engine
                .Received()
                .Run(
                    Arg.Is<SettingsContainer>(s =>
                        s.SourceControlServerSettings.Reviewers.Contains("notnukeeper@nukeeper.nukeeper")
                    )
                );
        }

        CollaborationPlatformCommand MakeCommand()
        {
            return new CollaborationPlatformCommandStub(
                _engine,
                _logger,
                _fileSettingsCache,
                _collaborationFactory
            )
            {
                ApiEndpoint = "http://tfs.myorganization.com/tfs/DefaultCollection/MyProject/_git/MyRepository",
                PersonalAccessToken = "mytoken"
            };
        }

        class CollaborationPlatformCommandStub : CollaborationPlatformCommand
        {
            public CollaborationPlatformCommandStub(
                ICollaborationEngine engine,
                IConfigureLogger logger,
                IFileSettingsCache fileSettingsCache,
                ICollaborationFactory collaborationFactory
            ) : base(engine, logger, fileSettingsCache, collaborationFactory) { }
        }
    }
}
