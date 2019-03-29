using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Logging;
using Model = NuKeeper.Gitlab.Model;

namespace NuKeeper.Gitlab
{
    public class GitlabRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public GitlabRestClient(HttpClient client, string token, INuKeeperLogger logger)
        {
            _client = client;
            _logger = logger;

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Private-Token", token);
        }

        // https://docs.gitlab.com/ee/api/users.html#for-normal-users-1
        // GET /user
        public async Task<Model.User> GetCurrentUser()
        {
            return await GetResource<Model.User>("user").ConfigureAwait(false);
        }

        // https://docs.gitlab.com/ee/api/projects.html#get-single-project
        // GET /projects/:id
        public async Task<Model.Project> GetProject(string projectName, string repositoryName)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");
            return await GetResource<Model.Project>($"projects/{encodedProjectName}").ConfigureAwait(false);
        }

        // https://docs.gitlab.com/ee/api/branches.html#get-single-repository-branch
        // GET /projects/:id/repository/branches/:branch
        public async Task<Model.Branch> CheckExistingBranch(string projectName, string repositoryName,
            string branchName)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");
            var encodedBranchName = HttpUtility.UrlEncode(branchName);

            return await GetResource<Model.Branch>(
                $"projects/{encodedProjectName}/repository/branches/{encodedBranchName}").ConfigureAwait(false);
        }

        // POST /projects/:id/merge_requests
        // https://docs.gitlab.com/ee/api/merge_requests.html#create-mr
        public Task<Model.MergeRequest> OpenMergeRequest(string projectName, string repositoryName, Model.MergeRequest mergeRequest)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");

            var content = new StringContent(JsonConvert.SerializeObject(mergeRequest), Encoding.UTF8,
                "application/json");
            return PostResource<Model.MergeRequest>($"projects/{encodedProjectName}/merge_requests", content);
        }

        private async Task<T> GetResource<T>(string url, [CallerMemberName] string caller = null)
        {
            var fullUrl = new Uri(url, UriKind.Relative);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.GetAsync(fullUrl).ConfigureAwait(false);
            return await HandleResponse<T>(response, caller).ConfigureAwait(false);
        }

        private async Task<T> PostResource<T>(string url, HttpContent content, [CallerMemberName] string caller = null)
        {
            _logger.Detailed($"{caller}: Requesting {url}");

            var response = await _client.PostAsync(url, content).ConfigureAwait(false);

            return await HandleResponse<T>(response, caller).ConfigureAwait(false);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response, [CallerMemberName] string caller = null)
        {
            string msg;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

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

                    case HttpStatusCode.NotFound:
                        return default;
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
            catch (Exception ex)
            {
                msg = $"{caller} failed to parse json to {typeof(T)}: {ex.Message}";
                _logger.Error(msg);
                throw new NuKeeperException($"Failed to parse json to {typeof(T)}", ex);
            }
        }
    }
}
