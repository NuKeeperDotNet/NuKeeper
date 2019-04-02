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
            return await GetResource<Model.User>("user");
        }

        // https://docs.gitlab.com/ee/api/projects.html#get-single-project
        // GET /projects/:id
        public async Task<Model.Project> GetProject(string projectName, string repositoryName)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");
            return await GetResource<Model.Project>($"projects/{encodedProjectName}");
        }

        // https://docs.gitlab.com/ee/api/branches.html#get-single-repository-branch
        // GET /projects/:id/repository/branches/:branch
        public async Task<Model.Branch> CheckExistingBranch(string projectName, string repositoryName,
            string branchName)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");
            var encodedBranchName = HttpUtility.UrlEncode(branchName);

            return await GetResource(
                $"projects/{encodedProjectName}/repository/branches/{encodedBranchName}",
                statusCode => statusCode == HttpStatusCode.NotFound
                    ? Result<Model.Branch>.Success(null)
                    : Result<Model.Branch>.Failure());
        }

        // https://docs.gitlab.com/ee/api/merge_requests.html#create-mr
        // POST /projects/:id/merge_requests
        public Task<Model.MergeRequest> OpenMergeRequest(string projectName, string repositoryName, Model.MergeRequest mergeRequest)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");

            var content = new StringContent(JsonConvert.SerializeObject(mergeRequest), Encoding.UTF8,
                "application/json");
            return PostResource<Model.MergeRequest>($"projects/{encodedProjectName}/merge_requests", content);
        }

        private async Task<T> GetResource<T>(string url, Func<HttpStatusCode, Result<T>> customErrorHandling = null, [CallerMemberName] string caller = null)
        {
            var fullUrl = new Uri(url, UriKind.Relative);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.GetAsync(fullUrl);
            return await HandleResponse(response, customErrorHandling, caller);
        }

        private async Task<T> PostResource<T>(string url, HttpContent content, Func<HttpStatusCode, Result<T>> customErrorHandling = null, [CallerMemberName] string caller = null)
        {
            _logger.Detailed($"{caller}: Requesting {url}");

            var response = await _client.PostAsync(url, content);

            return await HandleResponse(response, customErrorHandling, caller);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response,
            Func<HttpStatusCode, Result<T>> customErrorHandling,
            [CallerMemberName] string caller = null)
        {
            string msg;

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Detailed($"Response {response.StatusCode} is not success, body:\n{responseBody}");

                if (customErrorHandling != null)
                {
                    var result = customErrorHandling(response.StatusCode);
                    if (result.IsSuccessful)
                        return result.Value;
                }

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
            catch (Exception ex)
            {
                msg = $"{caller} failed to parse json to {typeof(T)}: {ex.Message}";
                _logger.Error(msg);
                throw new NuKeeperException($"Failed to parse json to {typeof(T)}", ex);
            }
        }
    }
}
