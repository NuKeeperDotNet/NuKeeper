using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ISettingsReader
    {
        Platform Platform { get; }

        Task<bool> CanRead(Uri repositoryUri);

        Task<RepositorySettings> RepositorySettings(Uri repositoryUri, string targetBranch = null, bool setAutoComplete = false);

        void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings);
    }
}
