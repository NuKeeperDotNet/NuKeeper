using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    //public class UserPermissions
    //{
    //    [JsonProperty("admin")]
    //    public bool IsAdmin { get; set; }

    //    [JsonProperty("pull")]
    //    public bool IsPull { get; set; }

    //    [JsonProperty("push")]
    //    public bool IsPush { get; set; }
    //}

    public class Permissions
    {
        public bool admin { get; set; }
        public bool push { get; set; }
        public bool pull { get; set; }
    }
}
