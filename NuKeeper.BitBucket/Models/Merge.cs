using System.Collections.Generic;

namespace NuKeeper.BitBucket.Models
{
    public class Merge
    {
        public string description { get; set; }
        public Links links { get; set; }
        public string title { get; set; }
        public bool? close_source_branch { get; set; }
        public List<object> reviewers { get; set; }
        public Source destination { get; set; }
        public string reason { get; set; }
        public User closed_by { get; set; }
        public User source { get; set; }
        public string state { get; set; }
        public User author { get; set; }
        public string created_on { get; set; }
        public List<object> participants { get; set; }
        public string updated_on { get; set; }
        public Commit merge_commit { get; set; }
        public int? id { get; set; }
    }
}
