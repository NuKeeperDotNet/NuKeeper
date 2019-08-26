using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Reviewer
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
