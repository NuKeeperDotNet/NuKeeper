using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class Permissions
    {
        [JsonProperty("project_access")]
        public Access ProjectAccess { get; set; }

        [JsonProperty("group_access")]
        public Access GroupAccess { get; set; }
    }
}
