using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationModels;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Abstractions.RepositoryInspection;
using NuKeeper.AzureDevOps;
using NuKeeper.BitBucket;
using NuKeeper.BitBucketLocal;
using NuKeeper.Engine;
using NuKeeper.Gitea;
using NuKeeper.GitHub;
using NuKeeper.Gitlab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NuKeeper.Collaboration
{
    public class CollaborationFactory : ICollaborationFactory
    {
        private readonly IEnumerable<ISettingsReader> _settingReaders;
        private readonly INuKeeperLogger _nuKeeperLogger;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITemplateValidator _templateValidator;
        private readonly IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> _enricher;
        private readonly IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate> _multiEnricher;

        private Platform? _platform;
        private string _commitTemplate;
        private string _pullrequestTitleTemplate;
        private string _pullrequestBodyTemplate;
        private IDictionary<string, object> _templateContext;

        public CollaborationFactory(
            IEnumerable<ISettingsReader> settingReaders,
            INuKeeperLogger nuKeeperLogger,
            IHttpClientFactory httpClientFactory,
            ITemplateValidator templateValidator,
            IEnrichContext<PackageUpdateSet, UpdateMessageTemplate> enricher,
            IEnrichContext<IReadOnlyCollection<PackageUpdateSet>, UpdateMessageTemplate> multiEnricher
        )
        {
            _settingReaders = settingReaders;
            _nuKeeperLogger = nuKeeperLogger;
            _httpClientFactory = httpClientFactory;
            _templateValidator = templateValidator;
            _enricher = enricher;
            _multiEnricher = multiEnricher;
            Settings = new CollaborationPlatformSettings();
        }

        public IForkFinder ForkFinder { get; private set; }
        public ICommitWorder CommitWorder { get; private set; }
        public IRepositoryDiscovery RepositoryDiscovery { get; private set; }
        public ICollaborationPlatform CollaborationPlatform { get; private set; }
        public CollaborationPlatformSettings Settings { get; }

        public async Task<ValidationResult> Initialise(
            Uri apiEndpoint,
            string token,
            ForkMode? forkModeFromSettings,
            Platform? platformFromSettings,
            string commitTemplate = null,
            string pullrequestTitleTemplate = null,
            string pullrequestBodyTemplate = null,
            IDictionary<string, object> templateContext = null
        )
        {
            _commitTemplate = commitTemplate;
            _pullrequestTitleTemplate = pullrequestTitleTemplate;
            _pullrequestBodyTemplate = pullrequestBodyTemplate;
            _templateContext = templateContext;

            var platformSettingsReader = await FindPlatformSettingsReader(platformFromSettings, apiEndpoint);
            if (platformSettingsReader != null)
            {
                _platform = platformSettingsReader.Platform;
            }
            else
            {
                return ValidationResult.Failure($"Unable to find collaboration platform for uri {apiEndpoint}");
            }

            Settings.BaseApiUrl = UriFormats.EnsureTrailingSlash(apiEndpoint);
            Settings.Token = token;
            Settings.ForkMode = forkModeFromSettings;
            platformSettingsReader.UpdateCollaborationPlatformSettings(Settings);

            var result = await ValidateSettings();
            if (!result.IsSuccess)
            {
                return result;
            }

            CreateForPlatform();

            return ValidationResult.Success;
        }

        private async Task<ISettingsReader> FindPlatformSettingsReader(
            Platform? platformFromSettings, Uri apiEndpoint)
        {
            if (platformFromSettings.HasValue)
            {
                var reader = _settingReaders
                    .FirstOrDefault(s => s.Platform == platformFromSettings.Value);

                if (reader != null)
                {
                    _nuKeeperLogger.Normal($"Collaboration platform specified as '{reader.Platform}'");
                }

                return reader;
            }
            else
            {
                var reader = await _settingReaders
                    .FirstOrDefaultAsync(s => s.CanRead(apiEndpoint));

                if (reader != null)
                {
                    _nuKeeperLogger.Normal($"Matched uri '{apiEndpoint}' to collaboration platform '{reader.Platform}'");
                }

                return reader;
            }
        }

        private async Task<ValidationResult> ValidateSettings()
        {
            if (!Settings.BaseApiUrl.IsWellFormedOriginalString()
                || (Settings.BaseApiUrl.Scheme != "http" && Settings.BaseApiUrl.Scheme != "https"))
            {
                return ValidationResult.Failure(
                    $"Api is not of correct format {Settings.BaseApiUrl}");
            }

            if (!Settings.ForkMode.HasValue)
            {
                return ValidationResult.Failure("Fork Mode was not set");
            }

            if (string.IsNullOrWhiteSpace(Settings.Token))
            {
                return ValidationResult.Failure("Token was not set");
            }

            if (!_platform.HasValue)
            {
                return ValidationResult.Failure("Platform was not set");
            }

            if (!string.IsNullOrEmpty(_commitTemplate))
            {
                var validationResult = await _templateValidator.ValidateAsync(_commitTemplate);
                if (!validationResult.IsSuccess)
                    return validationResult;
            }

            if (!string.IsNullOrEmpty(_pullrequestTitleTemplate))
            {
                var validationResult = await _templateValidator.ValidateAsync(_pullrequestTitleTemplate);
                if (!validationResult.IsSuccess)
                    return validationResult;
            }

            if (!string.IsNullOrEmpty(_pullrequestBodyTemplate))
            {
                var validationResult = await _templateValidator.ValidateAsync(_pullrequestBodyTemplate);
                if (!validationResult.IsSuccess)
                    return validationResult;
            }

            return ValidationResult.Success;
        }

        private void CreateForPlatform()
        {
            var forkMode = Settings.ForkMode.Value;

            UpdateMessageTemplate commitTemplate = new CommitUpdateMessageTemplate { CustomTemplate = _commitTemplate };
            UpdateMessageTemplate titleTemplate = null;
            UpdateMessageTemplate bodyTemplate = null;

            switch (_platform.Value)
            {
                case Platform.AzureDevOps:
                    CollaborationPlatform = new AzureDevOpsPlatform(_nuKeeperLogger, _httpClientFactory);
                    RepositoryDiscovery = new AzureDevOpsRepositoryDiscovery(_nuKeeperLogger, CollaborationPlatform, Settings.Token);
                    ForkFinder = new AzureDevOpsForkFinder(CollaborationPlatform, _nuKeeperLogger, forkMode);
                    titleTemplate = new AzureDevOpsPullRequestTitleTemplate { CustomTemplate = _pullrequestTitleTemplate };
                    bodyTemplate = new AzureDevOpsPullRequestBodyTemplate { CustomTemplate = _pullrequestBodyTemplate };
                    break;

                case Platform.GitHub:
                    CollaborationPlatform = new OctokitClient(_nuKeeperLogger);
                    RepositoryDiscovery = new GitHubRepositoryDiscovery(_nuKeeperLogger, CollaborationPlatform);
                    ForkFinder = new GitHubForkFinder(CollaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                case Platform.Bitbucket:
                    CollaborationPlatform = new BitbucketPlatform(_nuKeeperLogger, _httpClientFactory);
                    RepositoryDiscovery = new BitbucketRepositoryDiscovery(_nuKeeperLogger);
                    ForkFinder = new BitbucketForkFinder(CollaborationPlatform, _nuKeeperLogger, forkMode);
                    bodyTemplate = new BitbucketPullRequestBodyTemplate { CustomTemplate = _pullrequestBodyTemplate };
                    break;

                case Platform.BitbucketLocal:
                    CollaborationPlatform = new BitBucketLocalPlatform(_nuKeeperLogger, _httpClientFactory);
                    RepositoryDiscovery = new BitbucketLocalRepositoryDiscovery(_nuKeeperLogger, CollaborationPlatform, Settings);
                    ForkFinder = new BitbucketForkFinder(CollaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                case Platform.GitLab:
                    CollaborationPlatform = new GitlabPlatform(_nuKeeperLogger, _httpClientFactory);
                    RepositoryDiscovery = new GitlabRepositoryDiscovery(_nuKeeperLogger, CollaborationPlatform);
                    ForkFinder = new GitlabForkFinder(CollaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                case Platform.Gitea:
                    CollaborationPlatform = new GiteaPlatform(_nuKeeperLogger, _httpClientFactory);
                    RepositoryDiscovery = new GiteaRepositoryDiscovery(_nuKeeperLogger, CollaborationPlatform);
                    ForkFinder = new GiteaForkFinder(CollaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                default:
                    throw new NuKeeperException($"Unknown platform: {_platform}");
            }

            titleTemplate ??= new DefaultPullRequestTitleTemplate { CustomTemplate = _pullrequestTitleTemplate };
            bodyTemplate ??= new DefaultPullRequestBodyTemplate { CustomTemplate = _pullrequestBodyTemplate };

            InitializeTemplateContext(commitTemplate, _templateContext);
            InitializeTemplateContext(titleTemplate, _templateContext);
            InitializeTemplateContext(bodyTemplate, _templateContext);

            CommitWorder = new CommitWorder(
                commitTemplate,
                titleTemplate,
                bodyTemplate,
                _enricher,
                _multiEnricher
            );

            var auth = new AuthSettings(Settings.BaseApiUrl, Settings.Token, Settings.Username);
            CollaborationPlatform.Initialise(auth);

            if (ForkFinder == null ||
                RepositoryDiscovery == null ||
                CollaborationPlatform == null)
            {
                throw new NuKeeperException($"Platform {_platform} could not be initialised");
            }
        }

        private static void InitializeTemplateContext(UpdateMessageTemplate template, IDictionary<string, object> context)
        {
            template.Clear();

            if (context != null)
            {
                foreach (var property in context.Keys)
                {
                    template.AddPlaceholderValue(property, context[property], persist: true);
                }
            }
        }
    }
}
