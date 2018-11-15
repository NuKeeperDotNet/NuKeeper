using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NuKeeper.BitBucket.Models
{
    public class BuildInfo
    {
        public string name { get; set; }
        public string url { get; set; }
        public string refname { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public BuildInfoState state { get; set; }

        public string key { get; set; }
        public DateTime updated_on { get; set; }
        public DateTime created_on { get; set; }
        public string description { get; set; }
    }

    public enum BuildInfoState
    {
        SUCCESSFUL,
        FAILED,
        INPROGRESS,
        STOPPED
    }
}