using System;
using System.Collections.Generic;
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

namespace NuKeeper.Gitea
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
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
            var encodedProjectName = $"{ownerName}/{repositoryName}";
            return await GetResource<Repository>($"repos/{encodedProjectName}");
        }

        /// <summary>
        /// GET /admin/orgs https://try.gitea.io/api/swagger#/admin/adminGetAllOrgs
        /// </summary>
        /// <returns></returns>
        public async Task<List<Model.Organization>> GetOrganizations()
        {
            return await GetResource<List<Organization>>("admin/orgs");
        }

        /// <summary>
        /// /GET /orgs/{org}/repos List an organization's repos https://try.gitea.io/api/swagger#/organization/orgListRepos
        /// </summary>
        /// <param name="orgaName">name of the organization</param>
        /// <returns>list of repos </returns>
        public async Task<List<Repository>> GetOrgaRepositories(string orgaName)
        {
            return await GetResource<List<Repository>>($"/orgs/{orgaName}/repos");
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
            var encodedProjectName = $"{userName}/{repositoryName}";
            var encodedBranchName = HttpUtility.UrlEncode(branchName);

            return await GetResource<BranchInfo>(
                $"repos/{encodedProjectName}/branches/{encodedBranchName}",
            statusCode => statusCode == HttpStatusCode.NotFound
                    ? Result<Model.BranchInfo>.Success(null)
                    : Result<Model.BranchInfo>.Failure());
        }

        /// <summary>
        /// /POST /repos/{owner}/{repo}/forks https://try.gitea.io/api/swagger#/repository/createFork
        /// </summary>
        /// <param name="ownerName">owner of the repo to fork</param>
        /// <param name="repositoryName">name of the repo to fork</param>
        /// <returns></returns>
        public Task<Repository> ForkRepository(string ownerName, string repositoryName, string organizationName)
        {
            var encodedProjectName = $"{ownerName}/{repositoryName}";
            var content = new StringContent(JsonConvert.SerializeObject(new ForkInfo(organizationName)), Encoding.UTF8,
                "application/json");

            return PostResource<Repository>($"repos/{encodedProjectName}/forks", content);
        }

        /// <summary>
        /// /POST /repos/{owner}/{repo}/pulls Create a pull request
        /// </summary>
        /// <param name="owner">owner of the repo</param>
        /// <param name="repositoryName">name of the repository</param>
        /// <param name="pullRequest">pull request information</param>
        /// <returns></returns>
        public Task<PullRequest> OpenPullRequest(string owner, string repositoryName, CreatePullRequestOption pullRequest)
        {
            var encodedProjectName = $"{owner}/{repositoryName}";

            var content = new StringContent(JsonConvert.SerializeObject(pullRequest), Encoding.UTF8,
                "application/json");
            return PostResource<PullRequest>($"repos/{encodedProjectName}/pulls", content);
        }

        private async Task<T> GetResource<T>(string url, Func<HttpStatusCode, Result<T>> customErrorHandling = null, [CallerMemberName] string caller = null)
            where T : class
        {
            var fullUrl = new Uri(url, UriKind.Relative);
            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.GetAsync(fullUrl);
            return await HandleResponse<T>(response, customErrorHandling, caller);
        }

        private async Task<T> PostResource<T>(string url, HttpContent content, Func<HttpStatusCode, Result<T>> customErrorHandling = null, [CallerMemberName] string caller = null)
            where T : class
        {
            _logger.Detailed($"{caller}: Requesting {url}");

            var response = await _client.PostAsync(url, content);
            return await HandleResponse<T>(response, customErrorHandling, caller);
        }

        private async Task<T> HandleResponse<T>(HttpResponseMessage response,
            Func<HttpStatusCode, Result<T>> customErrorHandling,
            [CallerMemberName] string caller = null) where T : class
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
            catch (JsonException ex)
            {
                msg = $"{caller} failed to parse json to {typeof(T)}: {ex.Message}";
                _logger.Error(msg);
                throw new NuKeeperException($"Failed to parse json to {typeof(T)}", ex);
            }
        }
    }
}
