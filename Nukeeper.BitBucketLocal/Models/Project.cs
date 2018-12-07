using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Project
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }
    }
}
