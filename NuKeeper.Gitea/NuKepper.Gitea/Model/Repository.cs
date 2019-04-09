using Newtonsoft.Json;
using System;

namespace NuKeeper.Gitea.Model
{
    internal class Repository
    {
        [JsonProperty("archived")]
        public bool IsArchived { get; set; }

        [JsonProperty("clone_url")]
        public string CloneUrl { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("permissions")]
        public UserPermissions UserPermissions { get; }
        
        [JsonProperty("owner")]
        public User Owner { get; }

        [JsonProperty("fork")]
        public bool IsFork { get; }

        [JsonProperty("parent")]
        public Repository Parent { get; }
    }
}
