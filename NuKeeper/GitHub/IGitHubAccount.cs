namespace NuKeeper.GitHub
{
    public interface IGitHubAccount
    {
        string Login { get; }
        string Name { get; }
        string Email { get; }
    }
}
