using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace NuKeeper.Gitea.Model
{
    public class PullRequest
    {
        [JsonProperty("assignee")]
        public User Assignee { get; set; }

        [JsonProperty("assignees")]
        public IEnumerable<User> Assignees { get; set; }

        [JsonProperty("base")]
        public PullRequestBranchInfo Base { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("closed_at")]
        public DateTime CloseDate { get; set; }

        [JsonProperty("created_at")]
        public DateTime CreateDate { get; set; }

        [JsonProperty("due_date")]
        public DateTime DueDate { get; set; }

        [JsonProperty("comments")]
        public long Comments { get; set; }

        [JsonProperty("diff_url")]
        public string DiffUrl { get; set; }

        [JsonProperty("head")]
        public PullRequestBranchInfo Head { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("labels")]
        public IEnumerable<Label> Labels { get; set; }

        [JsonProperty("merge_base")]
        public string MergeBase { get; set; }

        [JsonProperty("merge_commit_sha")]
        public string MergeCommitSha { get; set; }

        [JsonProperty("mergeable")]
        public bool IsMergeable { get; set; }

        [JsonProperty("merged")]
        public bool IsMerged { get; set; }

        [JsonProperty("merged_at")]
        public DateTime MergedAt { get; set; }

        [JsonProperty("merged_by")]
        public User MergedBy { get; set; }

        [JsonProperty("number")]
        public long Number { get; set; }

        [JsonProperty("patch_url")]
        public string PatchUrl { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }
}
