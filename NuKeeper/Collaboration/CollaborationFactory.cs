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
        private ISettingsReader _settingsReader;

        public CollaborationPlatformSettings Settings { get; }

        public CollaborationFactory(IEnumerable<ISettingsReader> settingReaders,
            INuKeeperLogger nuKeeperLogger)
        {
            _settingReaders = settingReaders;
            _nuKeeperLogger = nuKeeperLogger;
            _settingsReader = null;
            Settings = new CollaborationPlatformSettings();
        }

        public void Initialise(Uri apiEndpoint, string token)
        {
            _settingsReader = _settingReaders
                .FirstOrDefault(s => s.CanRead(apiEndpoint));

            if (_settingsReader == null)
            {
                throw new NuKeeperException($"Unable to find collaboration platform for uri {apiEndpoint}");
            }

            _platform = _settingsReader.Platform;

            _nuKeeperLogger.Normal($"Matched uri '{apiEndpoint}' to collaboration platform '{_platform}'");

            Settings.BaseApiUrl = UriFormats.EnsureTrailingSlash(apiEndpoint);
            Settings.Token = token;
            _settingsReader.UpdateCollaborationPlatformSettings(Settings);
            ValidateSettings();
        }

        private void ValidateSettings()
        {
            if (!Settings.BaseApiUrl.IsWellFormedOriginalString()
                || (Settings.BaseApiUrl.Scheme != "http" && Settings.BaseApiUrl.Scheme != "https"))
            {
                throw new NuKeeperException($"Api is not of correct format {Settings.BaseApiUrl}");
            }
        }

        private IForkFinder _forkFinder;

        public IForkFinder ForkFinder
        {
            get
            {
                if (!_platform.HasValue)
                {
                    return null;
                }

                if (_forkFinder != null)
                {
                    return _forkFinder;
                }

                if (!Settings.ForkMode.HasValue)
                {
                    return null;
                }

                switch (_platform.Value)
                {
                    case Platform.AzureDevOps:
                        _forkFinder = new AzureDevOpsForkFinder(CollaborationPlatform, _nuKeeperLogger, Settings.ForkMode.Value);
                        break;
                    case Platform.GitHub:
                        _forkFinder = new GitHubForkFinder(CollaborationPlatform, _nuKeeperLogger, Settings.ForkMode.Value);
                        break;
                    case Platform.Bitbucket:
                        _forkFinder = new BitbucketForkFinder(CollaborationPlatform, _nuKeeperLogger, Settings.ForkMode.Value);
                        break;
                }

                return _forkFinder;
            }
        }

        private IRepositoryDiscovery _repositoryDiscovery;

        public IRepositoryDiscovery RepositoryDiscovery
        {
            get
            {
                if (!_platform.HasValue)
                {
                    return null;
                }

                if (_repositoryDiscovery != null)
                {
                    return _repositoryDiscovery;
                }

                switch (_platform.Value)
                {
                    case Platform.AzureDevOps:
                        _repositoryDiscovery = new AzureDevOpsRepositoryDiscovery(_nuKeeperLogger);
                        break;
                    case Platform.GitHub:
                        _repositoryDiscovery = new GitHubRepositoryDiscovery(_nuKeeperLogger, _collaborationPlatform);
                        break;
                    case Platform.Bitbucket:
                        _repositoryDiscovery = new BitbucketRepositoryDiscovery(_nuKeeperLogger);
                        break;
                }

                return _repositoryDiscovery;
            }
        }

        private ICollaborationPlatform _collaborationPlatform;

        public ICollaborationPlatform CollaborationPlatform
        {
            get
            {
                if (!_platform.HasValue)
                {
                    return null;
                }

                if (_collaborationPlatform != null)
                {
                    return _collaborationPlatform;
                }

                switch (_platform.Value)
                {
                    case Platform.AzureDevOps:
                        _collaborationPlatform = new AzureDevOpsPlatform(_nuKeeperLogger);
                        break;
                    case Platform.GitHub:
                        _collaborationPlatform = new OctokitClient(_nuKeeperLogger);
                        break;
                    case Platform.Bitbucket:
                        _collaborationPlatform = new BitbucketPlatform(_nuKeeperLogger);
                        break;
                }
                _collaborationPlatform?.Initialise(
                    new AuthSettings(Settings.BaseApiUrl, Settings.Token, Settings.Username)
                );

                return _collaborationPlatform;
            }
        }
    }
}
