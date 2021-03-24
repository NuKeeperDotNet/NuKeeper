using Newtonsoft.Json;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public AzureDevOpsRestClient(IHttpClientFactory clientFactory, INuKeeperLogger logger,
            string personalAccessToken, Uri apiBaseAddress)
        {
            _logger = logger;

            _client = clientFactory.CreateClient();
            _client.BaseAddress = apiBaseAddress;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{string.Empty}:{personalAccessToken}")));
        }

        private async Task<T> PostResource<T>(string url, HttpContent content, bool previewApi = false, [CallerMemberName] string caller = null)
        {
            var fullUrl = BuildAzureDevOpsUri(url, previewApi);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.PostAsync(fullUrl, content);
            return await HandleResponse<T>(response, caller);
        }

        private async Task<T> PatchResource<T>(string url, HttpContent content, bool previewApi = false, [CallerMemberName] string caller = null)
        {
            var fullUrl = BuildAzureDevOpsUri(url, previewApi);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod("PATCH"), fullUrl)
            {
                Content = content
            });
            return await HandleResponse<T>(response, caller);
        }

        private async Task<T> GetResource<T>(string url, bool previewApi = false, [CallerMemberName] string caller = null)
        {
            var fullUrl = BuildAzureDevOpsUri(url, previewApi);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.GetAsync(fullUrl);
            return await HandleResponse<T>(response, caller);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response, [CallerMemberName] string caller = null)
        {
            string msg;

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Detailed($"Response {response.StatusCode} is not success, body:\n{responseBody}");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        msg = $"{caller}: Unauthorised, ensure PAT has appropriate permissions";
                        _logger.Error(msg);
                        throw new NuKeeperException(msg);

                    case HttpStatusCode.Forbidden:
                        msg = $"{caller}: Forbidden, ensure PAT has appropriate permissions";
                        _logger.Error(msg);
                        throw new NuKeeperException(msg);

                    default:
                        msg = $"{caller}: Error {response.StatusCode}";
                        _logger.Error($"{caller}: Error {response.StatusCode}");
                        throw new NuKeeperException(msg);
                }
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
            catch (JsonException ex)
            {
                msg = $"{caller} failed to parse json to {typeof(T)}: {ex.Message}";
                _logger.Error(msg);
                throw new NuKeeperException($"Failed to parse json to {typeof(T)}", ex);
            }
        }

        public static Uri BuildAzureDevOpsUri(string relativePath, bool previewApi = false)
        {
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }

            var separator = relativePath.Contains("?") ? "&" : "?";
            return previewApi
                ? new Uri($"{relativePath}{separator}api-version=4.1-preview.1", UriKind.Relative)
                : new Uri($"{relativePath}{separator}api-version=4.1", UriKind.Relative);
        }

        // documentation is confusing, I think this won't work without memberId or ownerId
        // https://docs.microsoft.com/en-us/rest/api/azure/devops/account/accounts/list?view=azure-devops-rest-6.0
        public Task<Resource<Account>> GetCurrentUser()
        {
            return GetResource<Resource<Account>>("/_apis/accounts");
        }

        public Task<Resource<Account>> GetUserByMail(string email)
        {
            var encodedEmail = HttpUtility.UrlEncode(email);
            return GetResource<Resource<Account>>($"/_apis/identities?searchFilter=MailAddress&filterValue={encodedEmail}");
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var response = await GetResource<ProjectResource>("/_apis/projects");
            return response?.value.AsEnumerable();
        }

        public async Task<IEnumerable<AzureRepository>> GetGitRepositories(string projectName)
        {
            var response = await GetResource<GitRepositories>($"{projectName}/_apis/git/repositories");
            return response?.value.AsEnumerable();
        }

        public async Task<IEnumerable<GitRefs>> GetRepositoryRefs(string projectName, string repositoryId)
        {
            var response = await GetResource<GitRefsResource>($"{projectName}/_apis/git/repositories/{repositoryId}/refs");
            return response?.value.AsEnumerable();
        }

        //https://docs.microsoft.com/en-us/rest/api/azure/devops/git/pull%20requests/get%20pull%20requests?view=azure-devops-rest-5.0
        public async Task<IEnumerable<PullRequest>> GetPullRequests(
             string projectName,
             string azureRepositoryId,
             string headBranch,
             string baseBranch)
        {
            var encodedBaseBranch = HttpUtility.UrlEncode(baseBranch);
            var encodedHeadBranch = HttpUtility.UrlEncode(headBranch);

            var response = await GetResource<PullRequestResource>($"{projectName}/_apis/git/repositories/{azureRepositoryId}/pullrequests?searchCriteria.sourceRefName={encodedHeadBranch}&searchCriteria.targetRefName={encodedBaseBranch}");

            return response?.value.AsEnumerable();
        }

        public async Task<IEnumerable<PullRequest>> GetPullRequests(string projectName, string repositoryName, string user)
        {
            var response = await GetResource<PullRequestResource>(
                $"{projectName}/_apis/git/repositories/{repositoryName}/pullrequests?searchCriteria.creatorId={user}"
            );

            return response?.value.AsEnumerable();
        }

        public async Task<PullRequest> CreatePullRequest(PRRequest request, string projectName, string azureRepositoryId)
        {
            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            return await PostResource<PullRequest>($"{projectName}/_apis/git/repositories/{azureRepositoryId}/pullrequests", content);
        }

        public async Task<LabelResource> CreatePullRequestLabel(LabelRequest request, string projectName, string azureRepositoryId, int pullRequestId)
        {
            var labelContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            return await PostResource<LabelResource>($"{projectName}/_apis/git/repositories/{azureRepositoryId}/pullRequests/{pullRequestId}/labels", labelContent, true);
        }

        public async Task<PullRequest> SetAutoComplete(PRRequest request, string projectName, string azureRepositoryId, int pullRequestId)
        {
            var autoCompleteContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            return await PatchResource<PullRequest>($"{projectName}/_apis/git/repositories/{azureRepositoryId}/pullRequests/{pullRequestId}", autoCompleteContent);
        }
        
        public async Task<IEnumerable<string>> GetGitRepositoryFileNames(string projectName, string azureRepositoryId)
        {
            var response = await GetResource<GitItemResource>($"{projectName}/_apis/git/repositories/{azureRepositoryId}/items?recursionLevel=Full");
            return response?.value.Select(v => v.path).AsEnumerable();
        }
    }
}
