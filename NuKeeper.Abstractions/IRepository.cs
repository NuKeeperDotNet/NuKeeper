using System;

namespace NuKeeper.Abstractions
{
    public interface IRepository
    {
        IRepository Parent { get; }
        string Name { get; }
        RepositoryPermissions Permissions { get; }
        bool Archived { get; }
        Uri CloneUrl { get; }
        Uri HtmlUrl { get; }
        bool Fork { get; }
        User Owner { get; }
    }
}
