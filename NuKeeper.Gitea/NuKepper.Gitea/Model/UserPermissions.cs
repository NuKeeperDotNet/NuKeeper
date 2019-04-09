using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    internal class UserPermissions
    {
        [JsonProperty("admin")]
        public bool IsAdmin { get; set; }

        [JsonProperty("pull")]
        public bool IsPull { get; set; }

        [JsonProperty("push")]
        public bool IsPush { get; set; }
    }
}
