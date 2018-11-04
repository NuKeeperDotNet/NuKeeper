using System;
using System.Collections.Generic;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public interface ISettingsReader
    {
        RepositorySettings RepositorySettings(Uri repositoryUri);
        AuthSettings AuthSettings(string apiEndpoint, string accessToken);
    }
}
