using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class Organization
    {
        [JsonProperty("avatar_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }
    }
}
