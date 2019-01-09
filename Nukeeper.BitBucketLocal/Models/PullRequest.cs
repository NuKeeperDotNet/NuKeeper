using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class PullRequest
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; } = string.Empty;

        [JsonProperty("state")]
        public string State { get; set; } = "OPEN";

        [JsonProperty("open")]
        public bool Open { get; set; } = true;

        [JsonProperty("closed")]
        public bool Closed { get; set; } = false;

        [JsonProperty("fromRef")]
        public Ref FromRef { get; set; }

        [JsonProperty("toRef")]
        public Ref ToRef { get; set; }

        [JsonProperty("locked")]
        public bool Locked { get; set; } = false;

        [JsonProperty("reviewers")]
        public List<PullRequestReviewer> Reviewers { get; set; }
    }
}


