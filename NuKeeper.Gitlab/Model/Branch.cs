using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class Branch
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("merged")]
        public bool Merged { get; set; }

        [JsonProperty("protected")]
        public bool Protected { get; set; }

        [JsonProperty("default")]
        public bool Default { get; set; }

        [JsonProperty("developers_can_push")]
        public bool DevelopersCanPush { get; set; }

        [JsonProperty("developers_can_merge")]
        public bool DevelopersCanMerge { get; set; }

        [JsonProperty("can_push")]
        public bool CanPush { get; set; }

        [JsonProperty("commit")]
        public Commit Commit { get; set; }
    }
}