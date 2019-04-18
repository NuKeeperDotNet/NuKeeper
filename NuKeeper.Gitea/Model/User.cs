using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class User
    {
        [JsonProperty("avatur_url")]
        public string AvatarUrl { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("is_admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("login")]
        public string Login { get; set; }
    }
}
