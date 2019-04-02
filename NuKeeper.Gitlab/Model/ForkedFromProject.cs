using System;
using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class ForkedFromProject
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_with_namespace")]
        public string NameWithNamespace { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("path_with_namespace")]
        public string PathWithNamespace { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }

        [JsonProperty("ssh_url_to_repo")]
        public string SshUrlToRepo { get; set; }

        [JsonProperty("http_url_to_repo")]
        public Uri HttpUrlToRepo { get; set; }

        [JsonProperty("web_url")]
        public Uri WebUrl { get; set; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("license_url")]
        public Uri LicenseUrl { get; set; }

        [JsonProperty("star_count")]
        public long StarCount { get; set; }

        [JsonProperty("forks_count")]
        public long ForksCount { get; set; }

        [JsonProperty("last_activity_at")]
        public DateTimeOffset LastActivityAt { get; set; }
    }
}
