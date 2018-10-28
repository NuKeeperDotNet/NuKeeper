using System;

namespace NuKeeper.Abstractions.Configuration
{
    public interface ISettingsReader
    {
        IRepositorySettings ReadRepositorySettings(Uri gitHubRepositoryUri);
    }
}
