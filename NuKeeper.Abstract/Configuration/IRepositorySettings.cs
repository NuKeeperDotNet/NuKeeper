using System;

namespace NuKeeper.Abstract.Configuration
{
    public interface IRepositorySettings
    {
        Uri Uri { get; }
        string Owner { get; }
        string RepositoryName { get; }
    }
}
