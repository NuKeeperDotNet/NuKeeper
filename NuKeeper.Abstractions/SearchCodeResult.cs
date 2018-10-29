namespace NuKeeper.Abstractions
{
    public class SearchCodeResult
    {
        public SearchCodeResult(int totalCount)
        {
            TotalCount = totalCount;
        }

        public int TotalCount { get; }
    }
}
