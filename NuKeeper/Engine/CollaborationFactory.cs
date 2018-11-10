using NuKeeper.Abstractions.CollaborationPlatform;
using System;
using System.Collections.Generic;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Logging;
using NuKeeper.AzureDevOps;
using NuKeeper.GitHub;

namespace NuKeeper.Engine
{
    public class CollaborationFactory : ICollaborationFactory
    {
        private readonly IEnumerable<ISettingsReader> _settingReaders;
        private readonly ICollaborationPlatform _collaborationPlatform;
        private readonly INuKeeperLogger _nuKeeperLogger;

        public ISettingsReader SettingsReader { get; private set; }
        public CollaborationPlatformSettings Settings { get; }
        private Platform? _platform;


        public CollaborationFactory(IEnumerable<ISettingsReader> settingReaders, ICollaborationPlatform collaborationPlatform, INuKeeperLogger nuKeeperLogger)
        {
            _settingReaders = settingReaders;
            _collaborationPlatform = collaborationPlatform;
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
                        _forkFinder = new AzureDevOpsForkFinder(_collaborationPlatform, _nuKeeperLogger);
                        break;
                    case Platform.GitHub:
                        _forkFinder = new GitHubForkFinder(_collaborationPlatform, _nuKeeperLogger);
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
    }
}
