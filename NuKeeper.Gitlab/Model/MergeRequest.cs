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
    }
}
