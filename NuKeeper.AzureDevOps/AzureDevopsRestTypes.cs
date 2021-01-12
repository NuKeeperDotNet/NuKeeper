using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace NuKeeper.AzureDevOps
{
#pragma warning disable CA1056 // Uri properties should not be strings
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2227 // Collection properties should be read only

    public class Resource<T>
    {
        public int count { get; set; }
        public IEnumerable<T> value { get; set; }
    }

    public class Account
    {
        public string accountId { get; set; }
        public string accountName { get; set; }
        public string accountOwner { get; set; }
        public Dictionary<string, object> properties { get; set; }
        public string Mail
        {
            get
            {
                if (properties.ContainsKey("Mail"))
                {
                    switch (properties["Mail"])
                    {
                        case JObject mailObject:
                            return mailObject.Property("$value").Value.ToString();

                        case JProperty mailProp:
                            return mailProp.Value.ToString();

                        case string mailString:
                            return mailString;
                    }
                }

                return string.Empty;
            }
        }
    }

    public class Avatar
    {
        public string href { get; set; }
    }

    public class Links
    {
        public Avatar avatar { get; set; }
    }

    public class Creator
    {
        public string displayName { get; set; }
        public string url { get; set; }
        public Links _links { get; set; }
        public string id { get; set; }
        public string uniqueName { get; set; }
        public string imageUrl { get; set; }
        public string descriptor { get; set; }
    }

    public class GitRefs
    {
        public string name { get; set; }
        public string objectId { get; set; }
        public Creator creator { get; set; }
        public string url { get; set; }
    }

    public class GitRefsResource
    {
        public List<GitRefs> value { get; set; }
        public int count { get; set; }
    }

    public class PullRequestErrorResource
    {
        public string id { get; set; }
        public object innerException { get; set; }
        public string message { get; set; }
        public string typeName { get; set; }
        public string typeKey { get; set; }
        public int errorCode { get; set; }
        public int eventId { get; set; }
    }

    public class PullRequestResource
    {
        public int Count { get; set; }
        public IEnumerable<PullRequest> value { get; set; }
    }

    public class PullRequest
    {
        public AzureRepository AzureRepository { get; set; }
        public int PullRequestId { get; set; }
        public int CodeReviewId { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SourceRefName { get; set; }
        public string TargetRefName { get; set; }
        public string MergeStatus { get; set; }
        public string MergeId { get; set; }
        public string Url { get; set; }
        public bool SupportsIterations { get; set; }
        public Creator CreatedBy { get; set; }
        public IEnumerable<WebApiTagDefinition> labels { get; set; }

        // public CreatedBy CreatedBy { get; set; }
        // public Lastmergesourcecommit LastMergeSourceCommit { get; set; }
        // public Lastmergetargetcommit LastMergeTargetCommit { get; set; }
        // public Lastmergecommit LastMergeCommit { get; set; }
        // public IEnumerable<Reviewer> Reviewers { get; set; }
    }

    public class WebApiTagDefinition
    {
        public bool active { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class ProjectResource
    {
        public int Count { get; set; }
        public IEnumerable<Project> value { get; set; }
    }
    public class Project
    {
        public string description { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public string state { get; set; }
        public int revision { get; set; }
        public string visibility { get; set; }
    }
    public class PRRequest
    {
        public string sourceRefName { get; set; }
        public string targetRefName { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public GitPullRequestCompletionOptions completionOptions { get; set; }
        public Creator autoCompleteSetBy { get; set; }
    }
    public class GitPullRequestCompletionOptions
    {
        public bool deleteSourceBranch { get; set; }
    }

    public class AzureRepository
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Project project { get; set; }
        public string defaultBranch { get; set; }
        public long size { get; set; }
        public string remoteUrl { get; set; }
        public string sshUrl { get; set; }
    }

    public class GitRepositories
    {
        public IEnumerable<AzureRepository> value { get; set; }
        public int count { get; set; }
    }

    public class LabelRequest
    {
        public string name { get; set; }
    }

    public class Label
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public string url { get; set; }
    }

    public class LabelResource
    {
        public int count { get; set; }
        public IEnumerable<Label> value { get; set; }
    }

    public class GitItemResource
    {
        public int count { get; set; }
        public IEnumerable<GitItem> value { get; set; }
    }

    public class GitItem
    {
        public string path { get; set; }
    }
}
