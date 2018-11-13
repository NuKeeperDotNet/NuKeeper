namespace NuKeeper.BitBucket.Models
{
    public class Activity
    {
        public Update update { get; set; }
        public PullRequest pull_request { get; set; }
        public Comment comment { get; set; }
    }
}