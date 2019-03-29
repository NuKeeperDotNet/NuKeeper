using System;
using Newtonsoft.Json;

namespace NuKeeper.Gitlab.Model
{
    public class Commit
    {
        [JsonProperty("author_email")]
        public string AuthorEmail { get; set; }

        [JsonProperty("author_name")]
        public string AuthorName { get; set; }

        [JsonProperty("authored_date")]
        public DateTimeOffset AuthoredDate { get; set; }

        [JsonProperty("committed_date")]
        public DateTimeOffset CommittedDate { get; set; }

        [JsonProperty("committer_email")]
        public string CommitterEmail { get; set; }

        [JsonProperty("committer_name")]
        public string CommitterName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("short_id")]
        public string ShortId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
