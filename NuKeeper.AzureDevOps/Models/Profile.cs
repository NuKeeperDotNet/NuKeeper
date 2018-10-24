namespace NuKeeper.AzureDevOps.Models
{
    using System;
    using Newtonsoft.Json;

    public class AzureProfile
    {
        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("publicAlias")]
        public Guid PublicAlias { get; set; }

        [JsonProperty("emailAddress")]
        public string EmailAddress { get; set; }

        [JsonProperty("coreRevision")]
        public long CoreRevision { get; set; }

        [JsonProperty("timeStamp")]
        public DateTimeOffset TimeStamp { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("revision")]
        public long Revision { get; set; }
    }
}
