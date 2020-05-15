using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucketLocal.Models;

namespace NuKeeper.BitBucketLocal
{
    public class BitbucketLocalRestClient
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        private const string ApiPath = @"rest/api/1.0";
        private const string ApiReviewersPath = @"rest/default-reviewers/1.0";

        public BitbucketLocalRestClient(HttpClient client, INuKeeperLogger logger, string username, string appPassword)
        {
            _client = client;
            _logger = logger;

            var byteArray = Encoding.ASCII.GetBytes($"{username}:{appPassword}");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<T> GetResourceOrEmpty<T>(string url, [CallerMemberName] string caller = null)
        {
            _logger.Detailed($"Getting from BitBucketLocal url {url}");
            var response = await _client.GetAsync(url);
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
                        msg = $"{caller}: Unauthorised, ensure Username and Password are correct and user has appropriate permissions";
                        _logger.Error(msg);
                        throw new NuKeeperException(msg);

                    case HttpStatusCode.Forbidden:
                        msg = $"{caller}: Forbidden, ensure User has appropriate permissions";
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
            catch (JsonException)
            {
                msg = $"{caller}: Json exception";
                _logger.Error(msg);
                throw new NuKeeperException(msg);
            }
        }

        public async Task<User> GetCurrentUser(string username, [CallerMemberName] string caller = null)
        {
            var users = await GetResourceOrEmpty<IteratorBasedPage<User>>($@"{ApiPath}/users?filter={username}", caller);
            var user = users.Values.FirstOrDefault(x => x.Name.Equals(username, StringComparison.InvariantCultureIgnoreCase));
            return user;
        }

        public async Task<IEnumerable<Repository>> GetProjects([CallerMemberName] string caller = null)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Repository>>($@"{ApiPath}/projects?limit=999", caller);
            return response.Values;
        }

        public async Task<IEnumerable<Repository>> GetGitRepositories(string projectName, [CallerMemberName] string caller = null)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Repository>>($@"{ApiPath}/projects/{projectName}/repos?limit=999", caller);
            return response.Values;
        }

        public async Task<IEnumerable<string>> GetGitRepositoryFileNames(string projectName, string repositoryName, [CallerMemberName] string caller = null)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<string>>($@"{ApiPath}/projects/{projectName}/repos/{repositoryName}/files?limit=9999", caller);
            return response.Values;
        }

        public async Task<IEnumerable<Branch>> GetGitRepositoryBranches(string projectName, string repositoryName, [CallerMemberName] string caller = null)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Branch>>($@"{ApiPath}/projects/{projectName}/repos/{repositoryName}/branches", caller);
            return response.Values;
        }

        public async Task<IEnumerable<PullRequest>> GetPullRequests(
            string projectName,
            string repositoryName,
            string headBranch,
            string baseBranch,
            [CallerMemberName] string caller = null)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<PullRequest>>($@"{ApiPath}/projects/{projectName}/repos/{repositoryName}/pull-requests", caller);

            return response.Values
                .Where(p => p.Open
                && p.FromRef.Id.Equals(headBranch,StringComparison.InvariantCultureIgnoreCase)
                && p.ToRef.Id.Equals(baseBranch,StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<PullRequest> CreatePullRequest(PullRequest pullReq, string projectName, string repositoryName, [CallerMemberName] string caller = null)
        {
            var requestJson = JsonConvert.SerializeObject(pullReq, Formatting.None, JsonSerializerSettings);
            var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync($@"{ApiPath}/projects/{projectName}/repos/{repositoryName}/pull-requests", requestBody);

            return await HandleResponse<PullRequest>(response, caller);
        }

        public async Task<IEnumerable<PullRequestReviewer>> GetBitBucketReviewers(string projectName, string repositoryName, [CallerMemberName] string caller = null)
        {
            var response = await GetResourceOrEmpty<List<Conditions>>($@"{ApiReviewersPath}/projects/{projectName}/repos/{repositoryName}/conditions", caller);
            return response.SelectMany(c => c.Reviewers).Select(usr => new PullRequestReviewer() { User = usr });
        }
    }
}
