namespace NuKeeper.AzureDevOps.Models
{
    using Newtonsoft.Json;
    using System.Collections.Generic;

    public partial class PullRequest
    {
        [JsonProperty("sourceRefName")]
        public string SourceRefName { get; set; }

        [JsonProperty("targetRefName")]
        public string TargetRefName { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("reviewers")]
        public IEnumerable<Reviewer> Reviewers { get; set; }
    }
}
