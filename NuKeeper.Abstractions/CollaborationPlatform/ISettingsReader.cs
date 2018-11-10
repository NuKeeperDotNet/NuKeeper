using System;
using System.Collections.Generic;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ISettingsReader
    {
        Platform Platform { get; }
        bool CanRead(Uri repositoryUri);

        RepositorySettings RepositorySettings(Uri repositoryUri);
        AuthSettings AuthSettings(Uri apiUri, string accessToken);
    }
}
