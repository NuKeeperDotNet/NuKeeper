namespace NuKeeper.GitHub
{
    public interface IRepository
    {
        string CloneUrl { get; }
        string Name { get; }
        bool Fork { get; }
        string HtmlUrl { get; }
        bool Archived { get; }

        IGitHubAccount Owner { get; }
        IGitHubRepositoryPermissions Permissions { get; }
        IRepository Parent { get; }
    }
}
