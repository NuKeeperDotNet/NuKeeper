using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace NuKeeper.Gitea.Model
{
    public class CreatePullRequestOption
    {
        [JsonProperty("assignee")]
        public string Assignee { get; set; }

        [JsonProperty("base")]
        public string Base { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("due_date")]
        public DateTime DueDate { get; set; }

        public string Head { get; set; }

        [JsonProperty("milestone")]
        public long Milestone { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("assignees")]
        public IEnumerable<string> Assginees
        {
            get;
            set;
        }

        [JsonProperty("labels")]
        public IEnumerable<long> Labels { get; set; }
    }
}
