namespace NuKeeper.BitBucket.Models
{
    public class PullRequest
    {
        public string description { get; set; }
        public Links links { get; set; }
        public User author { get; set; }
        public bool? close_source_branch { get; set; }
        public string title { get; set; }
        public Source destination { get; set; }
        public string reason { get; set; }
        public object closed_by { get; set; }
        public Source source { get; set; }
        public string state { get; set; }
        public string created_on { get; set; }
        public string updated_on { get; set; }
        public object merge_commit { get; set; }
        public int? id { get; set; }
    }
}
