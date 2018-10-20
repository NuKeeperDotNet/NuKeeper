using System;
using Newtonsoft.Json;

namespace NuKeeper.AzureDevOps.Models
{
    public partial class CreatedBy
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }
}