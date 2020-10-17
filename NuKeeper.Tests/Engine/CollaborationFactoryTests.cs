using NSubstitute;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.AzureDevOps;
using NuKeeper.Collaboration;
using NuKeeper.Engine;
using NuKeeper.GitHub;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuKeeper.Tests.Engine
{
    [TestFixture]
    public class CollaborationFactoryTests
    {
        private const string _commitTemplate = "commit template";
        private const string _bodyTemplate = "body template";
        private const string _titleTemplate = "title template";
        private static readonly Dictionary<string, object> _context = new Dictionary<string, object>
        {
            { "company", "nukeeper" }
        };

        ITemplateValidator _templateValidator;

        [SetUp]
        public void Initialize()
        {
            _templateValidator = Substitute.For<ITemplateValidator>();
            _templateValidator.ValidateAsync(Arg.Any<string>()).Returns(ValidationResult.Success);
        }

        [Test]
        public void UnitialisedFactoryHasNulls()
        {
            var f = GetCollaborationFactory();

            Assert.That(f, Is.Not.Null);
            Assert.That(f.CollaborationPlatform, Is.Null);
            Assert.That(f.ForkFinder, Is.Null);
            Assert.That(f.RepositoryDiscovery, Is.Null);
        }

        [Test]
        public async Task UnknownApiReturnsUnableToFindPlatform()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                    new Uri("https://unknown.com/"), null,
                    ForkMode.SingleRepositoryOnly, null);

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.ErrorMessage,
                Is.EqualTo("Unable to find collaboration platform for uri https://unknown.com/"));
        }

        [Test]
        public async Task UnknownApiCanHaveManualPlatform()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                    new Uri("https://unknown.com/"), "token",
                    ForkMode.SingleRepositoryOnly,
                    Platform.GitHub);

            Assert.That(result.IsSuccess);
            AssertGithub(collaborationFactory);
        }

        [Test]
        public async Task ManualPlatformWillOverrideUri()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.myco.com"), "token",
                ForkMode.SingleRepositoryOnly,
                Platform.AzureDevOps);

            Assert.That(result.IsSuccess);
            AssertAzureDevOps(collaborationFactory);
        }

        [Test]
        public async Task AzureDevOpsUrlReturnsAzureDevOps()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://dev.azure.com"),
                "token",
                ForkMode.SingleRepositoryOnly,
                null,
                _commitTemplate,
                _titleTemplate,
                _bodyTemplate,
                _context
            );
            Assert.That(result.IsSuccess);

            AssertAzureDevOps(collaborationFactory);
            AssertCommitTempaltes(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        [Test]
        public async Task GithubUrlReturnsGitHub()
        {
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.com"),
                "token",
                ForkMode.PreferFork,
                null,
                _commitTemplate,
                _titleTemplate,
                _bodyTemplate,
                _context
            );
            Assert.That(result.IsSuccess);

            AssertGithub(collaborationFactory);
            AssertCommitTempaltes(collaborationFactory);
            AssertAreSameObject(collaborationFactory);
        }

        [Test]
        public async Task Initialise_ValidTemplates_ReturnsSuccessValidationResult()
        {
            _templateValidator
                .ValidateAsync(Arg.Any<string>())
                .Returns(ValidationResult.Success);
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.com"),
                "token",
                ForkMode.SingleRepositoryOnly,
                Platform.GitHub,
                "commit template",
                "pull request title template",
                "pull request body template"
            );

            Assert.That(result.IsSuccess, Is.True);
        }

        [Test]
        public async Task Initialise_InvalidCommitTemplate_ReturnsFailedValidationResult()
        {
            var commitTemplate = "invalid commit template";
            _templateValidator
                .ValidateAsync(commitTemplate)
                .Returns(ValidationResult.Failure("invalid template"));
            _templateValidator
                .ValidateAsync(Arg.Is<string>(s => s != commitTemplate))
                .Returns(ValidationResult.Success);
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.com"),
                "token",
                ForkMode.SingleRepositoryOnly,
                Platform.GitHub,
                commitTemplate,
                "pull request title template",
                "pull request body template"
            );

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task Initialise_InvalidPullRequestTitleTemplate_ReturnsFailedValidationResult()
        {
            var pullRequestTitleTemplate = "invalid pull request title template";
            _templateValidator
                .ValidateAsync(pullRequestTitleTemplate)
                .Returns(ValidationResult.Failure("invalid template"));
            _templateValidator
                .ValidateAsync(Arg.Is<string>(s => s != pullRequestTitleTemplate))
                .Returns(ValidationResult.Success);
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.com"),
                "token",
                ForkMode.SingleRepositoryOnly,
                Platform.GitHub,
                "commit template",
                pullRequestTitleTemplate,
                "invalid pull request body template"
            );

            Assert.That(result.IsSuccess, Is.False);
        }

        [Test]
        public async Task Initialise_InvalidPullRequestBodyTemplate_ReturnsFailedValidationResult()
        {
            var pullRequestBodyTemplate = "invalid pull request body template";
            _templateValidator
                .ValidateAsync(pullRequestBodyTemplate)
                .Returns(ValidationResult.Failure("invalid template"));
            _templateValidator
                .ValidateAsync(Arg.Is<string>(s => s != pullRequestBodyTemplate))
                .Returns(ValidationResult.Success);
            var collaborationFactory = GetCollaborationFactory();

            var result = await collaborationFactory.Initialise(
                new Uri("https://api.github.com"),
                "token",
                ForkMode.SingleRepositoryOnly,
                Platform.GitHub,
                "commit template",
                "pull request title template",
                pullRequestBodyTemplate
            );

            Assert.That(result.IsSuccess, Is.False);
        }

        private CollaborationFactory GetCollaborationFactory()
        {
            var azureUri = new Uri("https://dev.azure.com");
            var gitHubUri = new Uri("https://api.github.com");

            var settingReader1 = Substitute.For<ISettingsReader>();
            settingReader1.CanRead(azureUri).Returns(true);
            settingReader1.Platform.Returns(Platform.AzureDevOps);

            var settingReader2 = Substitute.For<ISettingsReader>();
            settingReader2.CanRead(gitHubUri).Returns(true);
            settingReader2.Platform.Returns(Platform.GitHub);

            var readers = new List<ISettingsReader> { settingReader1, settingReader2 };
            var logger = Substitute.For<INuKeeperLogger>();
            var httpClientFactory = Substitute.For<IHttpClientFactory>();
            httpClientFactory.CreateClient().Returns(new HttpClient());

            return new CollaborationFactory(
                readers,
                logger,
                httpClientFactory,
                _templateValidator,
                Substitute.For<IEnrichContext<PackageUpdateSet, UpdateMessageTemplate>>(),
                Substitute.For<IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate>>()
            );
        }

        private static void AssertAreSameObject(ICollaborationFactory collaborationFactory)
        {
            var collaborationPlatform = collaborationFactory.CollaborationPlatform;
            Assert.AreSame(collaborationPlatform, collaborationFactory.CollaborationPlatform);

            var repositoryDiscovery = collaborationFactory.RepositoryDiscovery;
            Assert.AreSame(repositoryDiscovery, collaborationFactory.RepositoryDiscovery);

            var forkFinder = collaborationFactory.ForkFinder;
            Assert.AreSame(forkFinder, collaborationFactory.ForkFinder);

            var settings = collaborationFactory.Settings;
            Assert.AreSame(settings, collaborationFactory.Settings);
        }

        private static void AssertGithub(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<GitHubForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<GitHubRepositoryDiscovery>(collaborationFactory.RepositoryDiscovery);
            Assert.IsInstanceOf<OctokitClient>(collaborationFactory.CollaborationPlatform);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
            Assert.IsInstanceOf<CommitWorder>(collaborationFactory.CommitWorder);

            var commitWorder = (CommitWorder)collaborationFactory.CommitWorder;
            Assert.IsInstanceOf<DefaultPullRequestTitleTemplate>(commitWorder.PullrequestTitleTemplate);
            Assert.IsInstanceOf<DefaultPullRequestBodyTemplate>(commitWorder.PullrequestBodyTemplate);
            Assert.IsInstanceOf<CommitUpdateMessageTemplate>(commitWorder.CommitTemplate);
        }

        private static void AssertAzureDevOps(ICollaborationFactory collaborationFactory)
        {
            Assert.IsInstanceOf<AzureDevOpsForkFinder>(collaborationFactory.ForkFinder);
            Assert.IsInstanceOf<AzureDevOpsRepositoryDiscovery>(collaborationFactory.RepositoryDiscovery);
            Assert.IsInstanceOf<AzureDevOpsPlatform>(collaborationFactory.CollaborationPlatform);
            Assert.IsInstanceOf<CollaborationPlatformSettings>(collaborationFactory.Settings);
            Assert.IsInstanceOf<CommitWorder>(collaborationFactory.CommitWorder);

            var commitWorder = (CommitWorder)collaborationFactory.CommitWorder;
            Assert.IsInstanceOf<AzureDevOpsPullRequestTitleTemplate>(commitWorder.PullrequestTitleTemplate);
            Assert.IsInstanceOf<AzureDevOpsPullRequestBodyTemplate>(commitWorder.PullrequestBodyTemplate);
            Assert.IsInstanceOf<CommitUpdateMessageTemplate>(commitWorder.CommitTemplate);
        }

        private static void AssertCommitTempaltes(ICollaborationFactory collaborationFactory)
        {
            var commitWorder = (CommitWorder)collaborationFactory.CommitWorder;
            Assert.AreEqual(_titleTemplate, commitWorder.PullrequestTitleTemplate.Value);
            Assert.AreEqual(_bodyTemplate, commitWorder.PullrequestBodyTemplate.Value);
            Assert.AreEqual(_commitTemplate, commitWorder.CommitTemplate.Value);
            Assert.AreEqual("nukeeper", commitWorder.PullrequestTitleTemplate.GetPlaceholderValue<string>("company"));
            Assert.AreEqual("nukeeper", commitWorder.PullrequestBodyTemplate.GetPlaceholderValue<string>("company"));
            Assert.AreEqual("nukeeper", commitWorder.CommitTemplate.GetPlaceholderValue<string>("company"));
        }
    }
}
