using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Branch
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("displayId")]
        public string DisplayId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("latestCommit")]
        public string LatestCommit { get; set; }

        [JsonProperty("latestChangeset")]
        public string LatestChangeset { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
    }
}
