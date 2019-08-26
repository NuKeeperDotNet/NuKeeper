using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    public class Conditions
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("reviewers")]
        public List<Reviewer> Reviewers { get; set; }
    }
}
