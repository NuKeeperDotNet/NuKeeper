using System;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICollaborationFactory
    {
        ValidationResult Initialise(Uri apiUri, string token, ForkMode? forkModeFromSettings);

        CollaborationPlatformSettings Settings { get; }
        IForkFinder ForkFinder { get; }
        IRepositoryDiscovery RepositoryDiscovery { get; }
        ICollaborationPlatform CollaborationPlatform { get; }
    }
}
