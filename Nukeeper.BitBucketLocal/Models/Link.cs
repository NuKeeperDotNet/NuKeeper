using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Link
    {
        [JsonProperty("href")]
        public string Href { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
