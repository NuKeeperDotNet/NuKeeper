using NuKeeper.Abstractions.CollaborationPlatform;
using System;
using System.Collections.Generic;
using NuKeeper.Abstractions;

namespace NuKeeper.Engine
{
    public class CollaborationFactory : ICollaborationFactory
    {
        private readonly IEnumerable<ISettingsReader> _settingReaders;
        public ISettingsReader SettingsReader { get; private set; }
        public CollaborationPlatformSettings Settings { get; }
        public Platform Platform { get; private set; }

        public CollaborationFactory(IEnumerable<ISettingsReader> settingReaders)
        {
            _settingReaders = settingReaders;
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
                    Platform = settingReader.Platform;
                }
            }

            if (SettingsReader == null)
            {
                throw new NuKeeperException("Unable to work out platform");
            }

            var settings = SettingsReader.AuthSettings(apiEndpoint, token);
            Settings.BaseApiUrl = settings.ApiBase;
            Settings.Token = settings.Token;
        }
    }
}
