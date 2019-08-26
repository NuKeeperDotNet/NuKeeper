namespace NuKeeper.BitBucket.Models
{
    public class PullRequestInfo
    {
        public string role { get; set; }
        public User user { get; set; }
        public bool? approved { get; set; }
    }
}
