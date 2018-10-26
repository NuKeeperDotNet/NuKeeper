namespace NuKeeper.AzureDevOps.Models
{
    using System;
    using System.Collections.Generic;
    using Newtonsoft.Json;

    public partial class GitPullRequest
    {
        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("pullRequestId")]
        public long PullRequestId { get; set; }

        [JsonProperty("codeReviewId")]
        public long CodeReviewId { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("createdBy")]
        public CreatedByClass CreatedBy { get; set; }

        [JsonProperty("creationDate")]
        public DateTimeOffset CreationDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("sourceRefName")]
        public string SourceRefName { get; set; }

        [JsonProperty("targetRefName")]
        public string TargetRefName { get; set; }

        [JsonProperty("mergeStatus")]
        public string MergeStatus { get; set; }

        [JsonProperty("mergeId")]
        public Guid MergeId { get; set; }

        [JsonProperty("lastMergeSourceCommit")]
        public LastMergeCommit LastMergeSourceCommit { get; set; }

        [JsonProperty("lastMergeTargetCommit")]
        public LastMergeCommit LastMergeTargetCommit { get; set; }

        [JsonProperty("reviewers")]
        public IEnumerable<Reviewer> Reviewers { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("_links")]
        public Links Links { get; set; }

        [JsonProperty("supportsIterations")]
        public bool SupportsIterations { get; set; }

        [JsonProperty("artifactId")]
        public string ArtifactId { get; set; }
    }
}
