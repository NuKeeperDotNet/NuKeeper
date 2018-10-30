namespace NuKeeper.Abstractions.DTOs
{
    public class SearchCodeResult
    {
        public SearchCodeResult(int totalCount)
        {
            TotalCount = totalCount;
        }

        public int TotalCount { get; set; }
    }
}
