using System;
using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class Owner
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }
    }
}
