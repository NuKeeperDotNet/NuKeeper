using System;
using Newtonsoft.Json;

namespace NuKeeper.AzureDevOps.Models
{
    public partial class LastMergeCommit
    {
        [JsonProperty("commitId")]
        public string CommitId { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}