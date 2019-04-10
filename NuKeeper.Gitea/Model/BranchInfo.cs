using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class BranchInfo
    {
        [JsonProperty("commit")]
        public Commit Commit { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }
}
