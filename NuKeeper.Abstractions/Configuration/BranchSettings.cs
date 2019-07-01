namespace NuKeeper.Abstractions.Configuration
{
    public class BranchSettings
    {
        public string BranchNameTemplate { get; set; }

        public bool DeleteBranchAfterMerge { get; set; }
    }
}
