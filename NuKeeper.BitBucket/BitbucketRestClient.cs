using Newtonsoft.Json;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucket.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Web;

namespace NuKeeper.BitBucket
{
    public class BitbucketRestClient
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public BitbucketRestClient(HttpClient client, INuKeeperLogger logger, string username, string appPassword)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger;

            var byteArray = Encoding.ASCII.GetBytes($"{username}:{appPassword}");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<User> GetCurrentUser()
        {
            return await GetResourceOrEmpty<User>("user");
        }

        private async Task<T> GetResourceOrEmpty<T>(string url)
        {
            _logger.Detailed($"Getting from BitBucket url {url}");
            var response = await _client.GetAsync(url);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Detailed($"Response {response.StatusCode} is not success, body:\n{responseBody}");
                return default;
            }

            return JsonConvert.DeserializeObject<T>(responseBody);
        }

        public async Task<IEnumerable<ProjectInfo>> GetProjects(string account)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<ProjectInfo>>($"teams/{account}/projects/");
            return response.values;
        }

        public async Task<IEnumerable<Repository>> GetGitRepositories(string account)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Repository>>($"repositories/{account}");
            return response.values;
        }

        public async Task<Repository> GetGitRepository(string account, string repositoryName)
        {
            var response = await GetResourceOrEmpty<Repository>($"repositories/{account}/{repositoryName}");
            return response;
        }

        public async Task<IEnumerable<Ref>> GetRepositoryRefs(string account, string repositoryId)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Ref>>($"repositories/{account}/{repositoryId}/refs");
            return response.values;
        }

        // https://developer.atlassian.com/bitbucket/api/2/reference/meta/filtering#query-pullreq
        public async Task<PullRequestsInfo> GetPullRequests(
            string account,
            string repositoryName,
            string headBranch,
            string baseBranch)
        {
            var filter = $"state =\"open\" AND source.branch.name = \"{headBranch}\" AND destination.branch.name = \"{baseBranch}\"";

            var response = await GetResourceOrEmpty<PullRequestsInfo>($"repositories/{account}/{repositoryName}/pullrequests?q={HttpUtility.UrlEncode(filter)}");

            return response;
        }

        public async Task<PullRequest> CreatePullRequest(string account, string repositoryName, PullRequest request)
        {
            if (request == null) throw new ArgumentException("Request cannot be null", nameof(request));

            //get the default reviewers defined in project to notify about new pull requests
            var reviewers = await GetResourceOrEmpty<IteratorBasedPage<User>>($"repositories/{account}/{repositoryName}/default-reviewers");

            if (reviewers.values.Any())
            {
                //Bitbucket API doesn't allow to set user as reviewer if this same user
                //is submiting a pull request
                var currentUserUUID = (await GetCurrentUser()).uuid;
                request.reviewers = reviewers.values.Where(r => r.uuid != currentUserUUID)
                                                    .Select(r => new PullRequestReviewer { uuid = r.uuid }).ToList();
            }

            var response = await _client.PostAsync($"repositories/{account}/{repositoryName}/pullrequests",
                                                   new StringContent(JsonConvert.SerializeObject(request, Formatting.None, JsonSerializerSettings),
                                                                     Encoding.UTF8,
                                                                     "application/json"));

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<PullRequest>(result);
            return resource;
        }
    }
}
