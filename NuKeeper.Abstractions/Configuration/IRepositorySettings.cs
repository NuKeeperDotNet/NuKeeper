using System;

namespace NuKeeper.Abstractions.Configuration
{
    public interface IRepositorySettings
    {
        Uri Uri { get; }
        string Owner { get; }
        string RepositoryName { get; }
    }
}
