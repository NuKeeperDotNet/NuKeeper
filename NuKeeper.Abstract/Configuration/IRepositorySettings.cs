using System;

namespace NuKeeper.Abstract.Configuration
{
    public interface IRepositorySettings
    {
        Uri Uri { get; set; }
        string Name { get; set; }
        string Owner { get; set; }
        string RepositoryName { get; }
    }
}
