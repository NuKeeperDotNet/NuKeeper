using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Reviewer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("active")]
        public bool Active { get; set; }
    }
}
