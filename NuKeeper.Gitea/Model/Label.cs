using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class Label
    {
        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
