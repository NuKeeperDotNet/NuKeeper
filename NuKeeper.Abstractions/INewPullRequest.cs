namespace NuKeeper.Abstractions
{
    public interface INewPullRequest
    {
        string Title { get; }
        string Head { get; }
        string BaseRef { get; }
    }
}
