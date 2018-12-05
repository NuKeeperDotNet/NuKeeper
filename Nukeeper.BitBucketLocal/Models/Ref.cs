using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Ref
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }
    }
}
