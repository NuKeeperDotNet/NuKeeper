using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class Actor
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }
    }
}
