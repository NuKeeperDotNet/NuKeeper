using Newtonsoft.Json;

namespace NuKeeper.Gitea.Model
{
    public class PullRequestBranchInfo
    {
        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("ref")]
        public string Ref { get; set; }

        [JsonProperty("repo")]
        public Repository Repo { get; set; }

        [JsonProperty("repo_id")]
        public long RepoId { get; set; }

        [JsonProperty("sha")]
        public string Sha { get; set; }
    }
}
