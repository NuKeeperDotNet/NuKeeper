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
using NuKeeper.Gitea.Model;

namespace NuKeeper.Gitlab
{
    public class GiteaRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public GiteaRestClient(HttpClient client, string token, INuKeeperLogger logger)
        {
            _client = client;
            _logger = logger;

            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Add("Private-Token", token);
        }

        /// <summary>
        /// GET /user https://try.gitea.io/api/swagger#/user/userGetCurrent
        /// </summary>
        /// <returns>returns the current user</returns>
        public async Task<User> GetCurrentUser()
        {
            return await GetResource<User>("user");
        }

        /// <summary>
        /// GET /repos/{owner}/{repo} https://try.gitea.io/api/swagger#/repository/repoGet
        /// </summary>
        /// <param name="ownerName"></param>
        /// <param name="repositoryName"></param>
        /// <returns></returns>
        public async Task<Repository> GetRepository(string ownerName, string repositoryName)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{ownerName}/{repositoryName}");
            return await GetResource<Repository>($"repos/{encodedProjectName}");
        }
        /// <summary>
        /// /GET /repos/{owner}/{repo}/branches/{branch} https://try.gitea.io/api/swagger#/repository/repoGetBranch
        /// </summary>
        /// <param name="userName">the owner</param>
        /// <param name="repositoryName">the repo name</param>
        /// <param name="branchName">branch to check</param>
        public async Task<BranchInfo> GetRepositoryBranch(string userName, string repositoryName,
            string branchName)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{userName}/{repositoryName}");
            var encodedBranchName = HttpUtility.UrlEncode(branchName);

            return await GetResource<BranchInfo>(
                $"repos/{encodedProjectName}/branches/{encodedBranchName}");
        }

        // https://docs.gitlab.com/ee/api/merge_requests.html#create-mr
        // POST /projects/:id/merge_requests
        public Task<MergeRequest> OpenMergeRequest(string projectName, string repositoryName, MergeRequest mergeRequest)
        {
            var encodedProjectName = HttpUtility.UrlEncode($"{projectName}/{repositoryName}");

            var content = new StringContent(JsonConvert.SerializeObject(mergeRequest), Encoding.UTF8,
                "application/json");
            return PostResource<MergeRequest>($"projects/{encodedProjectName}/merge_requests", content);
        }

        private async Task<T> GetResource<T>(string url, Func<HttpStatusCode, T, T> customErrorHandling = null, [CallerMemberName] string caller = null)
            where T : class
        {
            var fullUrl = new Uri(url, UriKind.Relative);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.GetAsync(fullUrl);
            return await HandleResponse<T>(response, caller);
        }

        private async Task<T> PostResource<T>(string url, HttpContent content, [CallerMemberName] string caller = null)
            where T : class
        {
            _logger.Detailed($"{caller}: Requesting {url}");

            var response = await _client.PostAsync(url, content);

            return await HandleResponse<T>(response, caller);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response, 
            [CallerMemberName] string caller = null) where T : class
        {
            string msg;

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Detailed($"Response {response.StatusCode} is not success, body:\n{responseBody}");

                //if (customErrorHandling != null)
                //{
                //    var result = customErrorHandling(response.StatusCode);

                //    if (result != null)
                //        return result;
                // }

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
    }
}
