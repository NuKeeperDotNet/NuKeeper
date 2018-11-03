using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;
        private readonly string _organisation;

        public AzureDevOpsRestClient(HttpClient client, INuKeeperLogger logger, string personalAccessToken, string organisation)
        {
            _client = client;
            _logger = logger;
            _organisation = organisation;
            _client.BaseAddress = new Uri($"https://dev.azure.com/");
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{string.Empty}:{personalAccessToken}")));
        }

        private async Task<T> GetResorceOrEmpty<T>(string url)
        {
            var fullUrl = BuildAzureDevOpsUri(url);
            var response = await _client.GetAsync(fullUrl);

            if (!response.IsSuccessStatusCode) return default;

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseBody);
        }
        private static Uri BuildAzureDevOpsUri(string relativePath)
        {
            return new Uri($"{relativePath}?api-version=4.1", UriKind.Relative);
        }
        public async Task<IEnumerable<Project>> GetProjects()
        {
            var response = await GetResorceOrEmpty<ProjectResource>($"/_apis/projects");
            return response.value.AsEnumerable();
        }

        public async Task<IEnumerable<AzureRepository>> GetGitRepositories(string projectName)
        {
            var response = await GetResorceOrEmpty<GitRepositories>($"/{_organisation}/{projectName}/_apis/git/repositories");
            return response.value.AsEnumerable();
        }

        public async Task<IEnumerable<GitRefs>> GetRepositoryRefs(string projectName,string repositoryId)
        {
            var response = await GetResorceOrEmpty<GitRefsResource>($"/{_organisation}/{projectName}/_apis/git/repositories/{repositoryId}/refs");
            return response.value.AsEnumerable();
        }

        public async Task<PullRequest> CreatePullRequest(PRRequest request, string projectName, string azureRepositoryId)
        {
            var response = await _client.PostAsync(BuildAzureDevOpsUri(($"/{_organisation}/{projectName}/_apis/git/repositories/{azureRepositoryId}/pullrequests")), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var pullRequestErrorResource = JsonConvert.DeserializeObject<PullRequestErrorResource>(error);
                _logger.Error(pullRequestErrorResource.message);
                return null;
            }

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<PullRequest>(result);
            return resource;
        }
    }

#pragma warning disable CA1056 // Uri properties should not be strings
#pragma warning disable CA1707 // Identifiers should not contain underscores
#pragma warning disable CA2227 // Collection properties should be read only


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
    public class PullRequest
    {
        public AzureRepository AzureRepository { get; set; }
        public int PullRequestId { get; set; }
        public int CodeReviewId { get; set; }
        public string Status { get; set; }
        // public CreatedBy CreatedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string SourceRefName { get; set; }
        public string TargetRefName { get; set; }
        public string MergeStatus { get; set; }
        public string MergeId { get; set; }

        // public Lastmergesourcecommit LastMergeSourceCommit { get; set; }
        // public Lastmergetargetcommit LastMergeTargetCommit { get; set; }
        //public Lastmergecommit LastMergeCommit { get; set; }
        // public IEnumerable<Reviewer> Reviewers { get; set; }
        #pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
        public bool SupportsIterations { get; set; }
    }
    public class ProjectResource
    {
        public int Count { get; set; }
        public IEnumerable<Project> value { get; set; }
    }
    public class Project
    {
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
    }
    public class AzureRepository
    {
        public string id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
        public Project project { get; set; }
        public string defaultBranch { get; set; }
        public int size { get; set; }
        public string remoteUrl { get; set; }
        public string sshUrl { get; set; }
    }

    public class GitRepositories
    {
        public IEnumerable<AzureRepository> value { get; set; }
        public int count { get; set; }
    }
}
