using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace NuKeeper.BitBucket.Models
{
   public partial class Refs
    {
        [JsonProperty("pagelen")]
        public long Pagelen { get; set; }

        [JsonProperty("values")]
        public List<Ref> Values { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("next")]
        public Uri Next { get; set; }
    }

    public partial class Ref
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("links")]
        public ValueLinks Links { get; set; }

        [JsonProperty("default_merge_strategy")]
        public MergeStrategy DefaultMergeStrategy { get; set; }

        [JsonProperty("merge_strategies")]
        public List<MergeStrategy> MergeStrategies { get; set; }

        [JsonProperty("type")]
        public ValueType Type { get; set; }

        [JsonProperty("target")]
        public Target Target { get; set; }
    }

    public partial class ValueLinks
    {
        [JsonProperty("commits")]
        public Commits Commits { get; set; }

        [JsonProperty("self")]
        public Commits Self { get; set; }

        [JsonProperty("html")]
        public Commits Html { get; set; }
    }

    public partial class Commits
    {
        [JsonProperty("href")]
        public Uri Href { get; set; }
    }

    public partial class Target
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("repository")]
        public Repository Repository { get; set; }

        [JsonProperty("links")]
        public TargetLinks Links { get; set; }

        [JsonProperty("author")]
        public Author Author { get; set; }

        [JsonProperty("parents")]
        public List<Parent> Parents { get; set; }

        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("type")]
        public ParentType Type { get; set; }
    }

    public partial class UserLinks
    {
        [JsonProperty("self")]
        public Commits Self { get; set; }

        [JsonProperty("html")]
        public Commits Html { get; set; }

        [JsonProperty("avatar")]
        public Commits Avatar { get; set; }
    }

    public partial class TargetLinks
    {
        [JsonProperty("self")]
        public Commits Self { get; set; }

        [JsonProperty("comments")]
        public Commits Comments { get; set; }

        [JsonProperty("patch", NullValueHandling = NullValueHandling.Ignore)]
        public Commits Patch { get; set; }

        [JsonProperty("html")]
        public Commits Html { get; set; }

        [JsonProperty("diff")]
        public Commits Diff { get; set; }

        [JsonProperty("approve")]
        public Commits Approve { get; set; }

        [JsonProperty("statuses")]
        public Commits Statuses { get; set; }
    }


    public enum MergeStrategy { FastForward, MergeCommit, Squash };

    public enum AuthorType { Author };

    public enum ParentType { Commit };

    public enum RepositoryType { Repository };

    public enum ValueType { Branch };
}