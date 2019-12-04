using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class MergeRequest
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("target_branch")]
        public string TargetBranch { get; set; }

        [JsonProperty("source_branch")]
        public string SourceBranch { get; set; }

        [JsonProperty("remove_source_branch")]
        public bool RemoveSourceBranch { get; set; }

        [JsonProperty("labels")]
        public IList<string> Labels { get; set; }
    }
}
