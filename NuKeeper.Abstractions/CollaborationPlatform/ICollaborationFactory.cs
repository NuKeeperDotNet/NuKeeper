using System;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICollaborationFactory
    {
        CollaborationPlatformSettings Settings { get; }
        ISettingsReader SettingsReader { get; }
        void Initialise(Uri apiUri, string token);
        IForkFinder ForkFinder { get; }
        IRepositoryDiscovery RepositoryDiscovery { get; }
    }
}
