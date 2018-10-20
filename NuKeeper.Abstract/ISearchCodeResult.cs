using System.Diagnostics.CodeAnalysis;

namespace NuKeeper.Abstract
{
    [SuppressMessage("ReSharper", "CA1040")]
    public interface ISearchCodeResult
    {
        int TotalCount { get; }
    }
}
