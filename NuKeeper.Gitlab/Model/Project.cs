using System;
using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class Project
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("description")]
        public object Description { get; set; }

        [JsonProperty("default_branch")]
        public string DefaultBranch { get; set; }

        [JsonProperty("visibility")]
        public string Visibility { get; set; }

        [JsonProperty("ssh_url_to_repo")]
        public string SshUrlToRepo { get; set; }

        [JsonProperty("http_url_to_repo")]
        public Uri HttpUrlToRepo { get; set; }

        [JsonProperty("web_url")]
        public Uri WebUrl { get; set; }

        [JsonProperty("readme_url")]
        public Uri ReadmeUrl { get; set; }

        [JsonProperty("owner")]
        public Owner Owner { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("name_with_namespace")]
        public string NameWithNamespace { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("path_with_namespace")]
        public string PathWithNamespace { get; set; }

        [JsonProperty("issues_enabled")]
        public bool IssuesEnabled { get; set; }

        [JsonProperty("open_issues_count")]
        public long OpenIssuesCount { get; set; }

        [JsonProperty("merge_requests_enabled")]
        public bool MergeRequestsEnabled { get; set; }

        [JsonProperty("jobs_enabled")]
        public bool JobsEnabled { get; set; }

        [JsonProperty("wiki_enabled")]
        public bool WikiEnabled { get; set; }

        [JsonProperty("snippets_enabled")]
        public bool SnippetsEnabled { get; set; }

        [JsonProperty("resolve_outdated_diff_discussions")]
        public bool ResolveOutdatedDiffDiscussions { get; set; }

        [JsonProperty("container_registry_enabled")]
        public bool ContainerRegistryEnabled { get; set; }

        [JsonProperty("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("last_activity_at")]
        public DateTimeOffset LastActivityAt { get; set; }

        [JsonProperty("creator_id")]
        public long CreatorId { get; set; }

        [JsonProperty("import_status")]
        public string ImportStatus { get; set; }

        [JsonProperty("archived")]
        public bool Archived { get; set; }

        [JsonProperty("avatar_url")]
        public Uri AvatarUrl { get; set; }

        [JsonProperty("shared_runners_enabled")]
        public bool SharedRunnersEnabled { get; set; }

        [JsonProperty("forks_count")]
        public long ForksCount { get; set; }

        [JsonProperty("star_count")]
        public long StarCount { get; set; }

        [JsonProperty("runners_token")]
        public string RunnersToken { get; set; }

        [JsonProperty("public_jobs")]
        public bool PublicJobs { get; set; }

        [JsonProperty("only_allow_merge_if_pipeline_succeeds")]
        public bool OnlyAllowMergeIfPipelineSucceeds { get; set; }

        [JsonProperty("only_allow_merge_if_all_discussions_are_resolved")]
        public bool OnlyAllowMergeIfAllDiscussionsAreResolved { get; set; }

        [JsonProperty("request_access_enabled")]
        public bool RequestAccessEnabled { get; set; }

        [JsonProperty("merge_method")]
        public string MergeMethod { get; set; }

        [JsonProperty("statistics")]
        public Statistics Statistics { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("import_error")]
        public object ImportError { get; set; }

        [JsonProperty("permissions", NullValueHandling = NullValueHandling.Ignore)]
        public Permissions Permissions { get; set; }

        [JsonProperty("forked_from_project", NullValueHandling = NullValueHandling.Ignore)]
        public ForkedFromProject ForkedFromProject { get; set; }
    }
}
