using System;
using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class Links
    {
        [JsonProperty("self")]
        public Uri Self { get; set; }

        [JsonProperty("issues")]
        public Uri Issues { get; set; }

        [JsonProperty("merge_requests")]
        public Uri MergeRequests { get; set; }

        [JsonProperty("repo_branches")]
        public Uri RepoBranches { get; set; }

        [JsonProperty("labels")]
        public Uri Labels { get; set; }

        [JsonProperty("events")]
        public Uri Events { get; set; }

        [JsonProperty("members")]
        public Uri Members { get; set; }
    }
}