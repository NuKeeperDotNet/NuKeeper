using Newtonsoft.Json;
using System;

namespace NuKeeper.Gitea.Model
{
    public class Repository
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

        //[JsonProperty("permissions")]
        //public UserPermissions UserPermissions { get; }

        //[JsonProperty("owner")]
        //public User Owner { get; }

        [JsonProperty("fork")]
        public bool IsFork { get; }

        //[JsonProperty("parent")]
        //public Repository Parent { get; }

        public int id { get; set; }
        public Owner owner { get; set; }
        //public string name { get; set; }
        public string full_name { get; set; }
        public string description { get; set; }
        public bool empty { get; set; }
        public bool @private { get; set; }
        //public bool fork { get; set; }
        public Repository parent { get; set; }
        public bool mirror { get; set; }
        public int size { get; set; }
        public string html_url { get; set; }
        public string ssh_url { get; set; }
        public string website { get; set; }
        public int stars_count { get; set; }
        public int forks_count { get; set; }
        public int watchers_count { get; set; }
        public int open_issues_count { get; set; }
        //public string default_branch { get; set; }
        //public DateTime created_at { get; set; }
        //public DateTime updated_at { get; set; }
        public Permissions permissions { get; set; }
    }
}
