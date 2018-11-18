using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NuKeeper.BitBucket;
using NuKeeper.GitHub;

namespace NuKeeper.Collaboration
{
    public class CollaborationFactory : ICollaborationFactory
    {
        private readonly IEnumerable<ISettingsReader> _settingReaders;
        private readonly INuKeeperLogger _nuKeeperLogger;
        private Platform? _platform;

        private IForkFinder _forkFinder;
        private IRepositoryDiscovery _repositoryDiscovery;
        private ICollaborationPlatform _collaborationPlatform;

        public IForkFinder ForkFinder => _forkFinder;

        public IRepositoryDiscovery RepositoryDiscovery => _repositoryDiscovery;

        public ICollaborationPlatform CollaborationPlatform => _collaborationPlatform;

        public CollaborationPlatformSettings Settings { get; }

        public CollaborationFactory(IEnumerable<ISettingsReader> settingReaders,
            INuKeeperLogger nuKeeperLogger)
        {
            _settingReaders = settingReaders;
            _nuKeeperLogger = nuKeeperLogger;
            Settings = new CollaborationPlatformSettings();
        }

        public void Initialise(Uri apiEndpoint, string token, ForkMode? forkModeFromSettings)
        {
            var platformSettingsReader = SettingsReaderForPlatform(apiEndpoint);

            _platform = platformSettingsReader.Platform;

            _nuKeeperLogger.Normal($"Matched uri '{apiEndpoint}' to collaboration platform '{_platform}'");

            Settings.BaseApiUrl = UriFormats.EnsureTrailingSlash(apiEndpoint);
            Settings.Token = token;
            Settings.ForkMode = forkModeFromSettings;
            platformSettingsReader.UpdateCollaborationPlatformSettings(Settings);

            ValidateSettings();
            CreateForPlatform();
        }

        private ISettingsReader SettingsReaderForPlatform(Uri apiEndpoint)
        {
            var platformSettingsReader = _settingReaders
                .FirstOrDefault(s => s.CanRead(apiEndpoint));

            if (platformSettingsReader == null)
            {
                throw new NuKeeperException($"Unable to find collaboration platform for uri {apiEndpoint}");
            }

            return platformSettingsReader;
        }

        private void ValidateSettings()
        {
            if (!Settings.BaseApiUrl.IsWellFormedOriginalString()
                || (Settings.BaseApiUrl.Scheme != "http" && Settings.BaseApiUrl.Scheme != "https"))
            {
                throw new NuKeeperException($"Api is not of correct format {Settings.BaseApiUrl}");
            }

            if (!Settings.ForkMode.HasValue)
            {
                throw new NuKeeperException("Fork Mode was not set");
            }

            if (!_platform.HasValue)
            {
                throw new NuKeeperException("Platform was not set");
            }
        }

        private void CreateForPlatform()
        {
            var forkMode = Settings.ForkMode.Value;

            switch (_platform.Value)
            {
                case Platform.AzureDevOps:
                    _collaborationPlatform = new AzureDevOpsPlatform(_nuKeeperLogger);
                    _repositoryDiscovery = new AzureDevOpsRepositoryDiscovery(_nuKeeperLogger);
                    _forkFinder = new AzureDevOpsForkFinder(_collaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                case Platform.GitHub:
                    _collaborationPlatform = new OctokitClient(_nuKeeperLogger);
                    _repositoryDiscovery = new GitHubRepositoryDiscovery(_nuKeeperLogger, _collaborationPlatform);
                    _forkFinder = new GitHubForkFinder(_collaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                case Platform.Bitbucket:
                    _collaborationPlatform = new BitbucketPlatform(_nuKeeperLogger);
                    _repositoryDiscovery = new BitbucketRepositoryDiscovery(_nuKeeperLogger);
                    _forkFinder = new BitbucketForkFinder(_collaborationPlatform, _nuKeeperLogger, forkMode);
                    break;

                default:
                    throw new NuKeeperException($"Unknown platform: {_platform}");
            }

            var auth = new AuthSettings(Settings.BaseApiUrl, Settings.Token, Settings.Username);
            _collaborationPlatform.Initialise(auth);

            if (_forkFinder == null ||
                _repositoryDiscovery == null ||
                _collaborationPlatform == null)
            {
                throw new NuKeeperException($"Platform {_platform} could not be initialised");
            }
        }
    }
}
