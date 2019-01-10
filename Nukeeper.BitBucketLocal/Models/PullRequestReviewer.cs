using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class PullRequestReviewer
    {
        [JsonProperty("user")]
        public Reviewer User { get; set; }
    }
}
