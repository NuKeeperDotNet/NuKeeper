using Newtonsoft.Json;
using System;

namespace NuKeeper.Gitea.Model
{
    public class Commit
    {

        [JsonProperty("author")]
        public Actor Author { get; set; }

        [JsonProperty("comitter")]
        public Actor Comitter { get; set; }

        [JsonProperty("timestamp")]
        public DateTime AuthoredDate { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("short_id")]
        public string ShortId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
