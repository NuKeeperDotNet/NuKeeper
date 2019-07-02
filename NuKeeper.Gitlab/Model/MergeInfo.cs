using Newtonsoft.Json;
using System;

namespace NuKeeper.Gitlab.Model
{
    public class MergeInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("iid")]
        public long Iid { get; set; }

        [JsonProperty("project_id")]
        public long ProjectId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("created_at")]
        public DateTime CretedAt { get; set; }

        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("web_url")]
        public Uri WebUrl { get; set; }

    }
}
