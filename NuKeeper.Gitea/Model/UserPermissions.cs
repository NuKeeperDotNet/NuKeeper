using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class Permissions
    {
        [JsonProperty("admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("pull")]
        public bool IsPull { get; set; }

        [JsonProperty("push")]
        public bool IsPush { get; set; }
    }
}
