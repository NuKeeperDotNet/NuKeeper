namespace NuKeeper.GitHub
{
    public interface IGitHubRepositoryPermissions
    {
        bool Push { get; }
        bool Pull { get; }
    }
}
