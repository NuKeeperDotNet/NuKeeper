using Newtonsoft.Json;

namespace NuKeeper.AzureDevOps.Models
{
    public partial class Links
    {
        [JsonProperty("self")]
        public CreatedBy Self { get; set; }

        [JsonProperty("repository")]
        public CreatedBy Repository { get; set; }

        [JsonProperty("workItems")]
        public CreatedBy WorkItems { get; set; }

        [JsonProperty("sourceBranch")]
        public CreatedBy SourceBranch { get; set; }

        [JsonProperty("targetBranch")]
        public CreatedBy TargetBranch { get; set; }

        [JsonProperty("sourceCommit")]
        public CreatedBy SourceCommit { get; set; }

        [JsonProperty("targetCommit")]
        public CreatedBy TargetCommit { get; set; }

        [JsonProperty("createdBy")]
        public CreatedBy CreatedBy { get; set; }

        [JsonProperty("iterations")]
        public CreatedBy Iterations { get; set; }
    }
}