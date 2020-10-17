using NuKeeper.Abstractions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ICollaborationFactory
    {
        Task<ValidationResult> Initialise(
            Uri apiUri,
            string token,
            ForkMode? forkModeFromSettings,
            Platform? platformFromSettings,
            string commitTemplate = null,
            string pullrequestTitleTemplate = null,
            string pullrequestBodyTemplate = null,
            IDictionary<string, object> templateContext = null
        );

        ICommitWorder CommitWorder { get; }
        CollaborationPlatformSettings Settings { get; }
        IForkFinder ForkFinder { get; }
        IRepositoryDiscovery RepositoryDiscovery { get; }
        ICollaborationPlatform CollaborationPlatform { get; }
    }
}
