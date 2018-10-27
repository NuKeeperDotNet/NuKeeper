using System;

namespace NuKeeper.Abstract.Configuration
{
    public interface ISettingsReader
    {
        IRepositorySettings ReadRepositorySettings(Uri gitHubRepositoryUri);
    }
}
