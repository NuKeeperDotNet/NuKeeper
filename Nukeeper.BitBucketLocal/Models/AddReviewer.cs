using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class AddReviewer
    {
        [JsonProperty("user")]
        public Reviewer User { get; set; }
    }
}