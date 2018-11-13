using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucket.Models;

namespace NuKeeper.BitBucket
{
    public class BitbucketRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public BitbucketRestClient(HttpClient client, INuKeeperLogger logger, string username, string appPassword)
        {
            _client = client;
            _logger = logger;

            var byteArray = Encoding.ASCII.GetBytes($"{username}:{appPassword}");
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<T> GetResourceOrEmpty<T>(string url)
        {
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode) return default;

            var responseBody = await response.Content.ReadAsStringAsync();
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

        public async Task<IEnumerable<Ref>> GetRepositoryRefs(string account, string repositoryId)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Ref>>($"repositories/{account}/{repositoryId}/refs");
            return response.values;
        }

        public async Task<PullRequest> CreatePullRequest(PullRequest request, string account, string reponame)
        {
            var response = await _client.PostAsync(($"repositories/{account}/{reponame}/pullrequests"),
                new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));

            var result = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<PullRequest>(result);
            return resource;
        }
    }
}