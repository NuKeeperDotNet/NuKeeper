using System.Diagnostics.CodeAnalysis;

namespace NuKeeper.Abstract
{
    [SuppressMessage("ReSharper", "CA1040")]
    public interface INewPullRequest
    {
        string Title { get; }
        string Head { get; }
    }
}
