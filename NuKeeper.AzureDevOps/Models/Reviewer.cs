using System;
using Newtonsoft.Json;

namespace NuKeeper.AzureDevOps.Models
{
    public partial class Reviewer
    {
        [JsonProperty("reviewerUrl")]
        public Uri ReviewerUrl { get; set; }

        [JsonProperty("vote")]
        public long Vote { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("uniqueName")]
        public string UniqueName { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }
    }
}
