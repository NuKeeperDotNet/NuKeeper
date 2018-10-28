using System.Diagnostics.CodeAnalysis;

namespace NuKeeper.Abstract
{
    public interface INewPullRequest
    {
        string Title { get; }
        string Head { get; }
        string BaseRef { get; }
    }
}
