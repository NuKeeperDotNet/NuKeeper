using NuKeeper.Abstractions.CollaborationPlatform;
using System;
using System.Collections.Generic;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NuKeeper.GitHub;

namespace NuKeeper.Engine
{
    public class CollaborationFactory : ICollaborationFactory
    {
        private readonly IEnumerable<ISettingsReader> _settingReaders;
        private readonly INuKeeperLogger _nuKeeperLogger;
        private Platform? _platform;

        public ISettingsReader SettingsReader { get; private set; }
        public CollaborationPlatformSettings Settings { get; }

        public CollaborationFactory(IEnumerable<ISettingsReader> settingReaders, INuKeeperLogger nuKeeperLogger)
        {
            _settingReaders = settingReaders;
            _nuKeeperLogger = nuKeeperLogger;
            SettingsReader = null;
            Settings = new CollaborationPlatformSettings();
        }

        public void Initialise(Uri apiEndpoint, string token)
        {
            foreach (var settingReader in _settingReaders)
            {
                if (settingReader.CanRead(apiEndpoint))
                {
                    SettingsReader = settingReader;
                    _platform = settingReader.Platform;
                }
            }

            if (SettingsReader == null)
            {
                throw new NuKeeperException($"Unable to work out platform for uri {apiEndpoint}");
            }

            var settings = SettingsReader.AuthSettings(apiEndpoint, token);
            Settings.BaseApiUrl = settings.ApiBase;
            Settings.Token = settings.Token;
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

                switch (_platform.Value)
                {
                    case Platform.AzureDevOps:
                        _forkFinder = new AzureDevOpsForkFinder(CollaborationPlatform, _nuKeeperLogger);
                        break;
                    case Platform.GitHub:
                        _forkFinder = new GitHubForkFinder(CollaborationPlatform, _nuKeeperLogger);
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
                }
                _collaborationPlatform?.Initialise(
                    new AuthSettings(Settings.BaseApiUrl, Settings.Token)
                );

                return _collaborationPlatform;
            }
        }

    }
}
