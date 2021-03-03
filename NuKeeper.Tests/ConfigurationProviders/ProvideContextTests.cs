using NSubstitute;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Commands;
using NuKeeper.ConfigurationProviders;
using NuKeeper.Inspection.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Tests.ConfigurationProviders
{
    [TestFixture]
    public class ProvideContextTests
    {
        private IConfigureLogger _logger;
        private IFileSettingsCache _fileSettings;

        [SetUp]
        public void Initialize()
        {
            _logger = Substitute.For<IConfigureLogger>();
            _fileSettings = Substitute.For<IFileSettingsCache>();

            _fileSettings.GetSettings().Returns(FileSettings.Empty());
        }

        [Test]
        public async Task ProvideAsync_NoContext_ReturnsSuccessWithEmptyContext()
        {
            var command = MakeCommand();
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, settingsContainer.UserSettings.Context.Count);
        }

        [Test]
        public async Task ProvideAsync_ContextFromCli_CorrectlyPopulatesSettingsContainerWithContext()
        {
            var command = MakeCommand();
            command.Context = new string[] { "issue=JIRA-001" };
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(
                settingsContainer.UserSettings.Context,
                new Dictionary<string, object>()
                {
                    { "issue", "JIRA-001" }
                }
            );
        }

        [Test]
        public async Task ProvideAsync_ContextFromFile_CorrectlyPopulatesSettingsContainerWithContext()
        {
            _fileSettings
                .GetSettings()
                .Returns(
                    new FileSettings
                    {
                        Context = new Dictionary<string, object> { { "issue", "JIRA-001" } }
                    }
                );
            var command = MakeCommand();
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(
                settingsContainer.UserSettings.Context,
                new Dictionary<string, object>()
                {
                    { "issue", "JIRA-001" }
                }
            );
        }

        [Test]
        public async Task ProvideAsync_ContextFromCliAndFile_CorrectlyPopulatesSettingsContainerWithContextFromCli()
        {
            _fileSettings
                .GetSettings()
                .Returns(
                    new FileSettings
                    {
                        Context = new Dictionary<string, object> { { "notwhatiwant", "tosee" } }
                    }
                );
            var command = MakeCommand();
            command.Context = new string[] { "issue=JIRA-001" };
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(
                settingsContainer.UserSettings.Context,
                new Dictionary<string, object>()
                {
                    { "issue", "JIRA-001" }
                }
            );
        }

        [TestCase("valid=property", true)]
        [TestCase("my:invalidproperty", false)]
        [TestCase("my invalidproperty", false)]
        public async Task ProvideAsync_ValidAndInvalidKeyValuePairs_AreAcceptedAndRejectedAsExpected(
            string setting,
            bool expectedOutcome
        )
        {
            var command = MakeCommand();
            command.Context = new string[] { setting };
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.AreEqual(expectedOutcome, result.IsSuccess);
        }

        [Test]
        public async Task ProvideAsync_SpecialDelegatesDictionaryPropertyFromCli_ParsesDelegatesFromStrings()
        {
            var command = MakeCommand();
            command.Context = new string[]
            {
                @"_delegates={ ""50char"":
                    ""
                        using System;
                        new Func<string, Func<string, string>, object>(
                            (str, render) =>
                            {
                                var rendering = render(str);
                                return rendering.Length > 50 ?
                                    rendering.Substring(0, 47).PadRight(50, '.')
                                    : rendering;
                            }
                        )
                    ""
                }"
            };
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsInstanceOf(
                typeof(Func<string, Func<string, string>, object>),
                settingsContainer.UserSettings.Context["50char"]
            );
        }

        [Test]
        public async Task ProvideAsync_SpecialDelegatesDictionaryPropertyFromFile_ParsesDelegatesFromStrings()
        {
            _fileSettings.GetSettings().Returns(
                new FileSettings
                {
                    Context = new Dictionary<string, object>
                    {
                        {
                            "_delegates", @"{ ""50char"":
                                ""
                                    using System;
                                    new Func<string, Func<string, string>, object>(
                                        (str, render) =>
                                        {
                                            var rendering = render(str);
                                            return rendering.Length > 50 ?
                                                rendering.Substring(0, 47).PadRight(50, '.')
                                                : rendering;
                                        }
                                    )
                                ""
                            }"
                        }
                    }
                }
            );
            var command = MakeCommand();
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsInstanceOf(
                typeof(Func<string, Func<string, string>, object>),
                settingsContainer.UserSettings.Context["50char"]
            );
        }

        [Test]
        public async Task ProvideAsync_InvalidCSharpExpressionForDelegate_ReturnsFailure()
        {
            var command = MakeCommand();
            command.Context = new string[]
            {
                @"_delegates={ ""50char"":
                    ""
                        This is not a csharp expression
                    ""
                }"
            };
            var configProvider = MakeProvideCommitContext(command);
            var settingsContainer = MakeEmptySettingsContainer();

            var result = await configProvider.ProvideAsync(settingsContainer);

            Assert.IsFalse(result.IsSuccess);
        }

        private CommandBase MakeCommand()
        {
            return new CommandBaseStub(_logger, _fileSettings);
        }

        private ProvideContext MakeProvideCommitContext(CommandBase command)
        {
            return new ProvideContext(_fileSettings, command);
        }

        private static SettingsContainer MakeEmptySettingsContainer()
        {
            return new SettingsContainer()
            {
                BranchSettings = new BranchSettings(),
                PackageFilters = new FilterSettings(),
                SourceControlServerSettings = new SourceControlServerSettings(),
                UserSettings = new UserSettings()
            };
        }

        class CommandBaseStub : CommandBase
        {
            public CommandBaseStub(
                IConfigureLogger logger,
                IFileSettingsCache fileSettingsCache
            ) : base(logger, fileSettingsCache) { }

            protected override Task<int> Run(SettingsContainer settings)
            {
                return Task.FromResult(0);
            }
        }
    }
}
