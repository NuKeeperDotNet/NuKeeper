using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuKeeper.BitBucketLocal.Models
{
    /// <summary>
    /// Models a page of data.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class IteratorBasedPage<T>
    {
        [JsonProperty("size")]
        public ulong? Size { get; set; }

        [JsonProperty("limit")]
        public int Limit { get; set; }
        [JsonProperty("isLastPage")]
        public bool IsLastPage { get; set; }

        [JsonProperty("values")]
        public List<T> Values { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("nextPageStart")]
        public int? NextPageStart { get; set; }
    }
}
