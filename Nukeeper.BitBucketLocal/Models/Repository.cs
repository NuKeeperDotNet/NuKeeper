using System;
using Newtonsoft.Json;
using NuGet.Common;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Repository
    {
        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("scmId")]
        public string ScmId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        public bool? Forkable { get; set; }

        [JsonProperty("project")]
        public Project Project { get; set; }

        [JsonProperty("public")]
        public bool? Public { get; set; }

        [JsonProperty("links")]
        public Links Links { get; set; }
    }
}

