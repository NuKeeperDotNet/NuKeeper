using System;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ISettingsReader
    {
        Platform Platform { get; }

        bool CanRead(Uri repositoryUri);

        RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch = null);

        void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings);
    }
}
