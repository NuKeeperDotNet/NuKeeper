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

        [JsonProperty("fork")]
        public bool IsFork { get; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("full_name")]
        public string FullName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("empty")]
        public bool IsEmpty { get; set; }

        [JsonProperty("parent")]
        public Repository Parent { get; set; }

        [JsonProperty("mirror")]
        public bool IsMirror { get; set; }

        [JsonProperty("size")]
        public int Size { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("ssh_url")]
        public string SshUrl { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("stars_count")]
        public int StarsCount { get; set; }

        [JsonProperty("forks_count")]
        public int ForksCount { get; set; }

        [JsonProperty("watchers_count")]
        public int WatchersCount { get; set; }

        [JsonProperty("open_issues_count")]
        public int OpenIssuesCount { get; set; }

        [JsonProperty("permissions")]
        public Permissions Permissions { get; set; }
    }
}
