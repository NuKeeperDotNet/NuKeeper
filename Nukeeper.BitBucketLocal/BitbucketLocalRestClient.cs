using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuKeeper.Abstractions.Logging;
using NuKeeper.BitBucketLocal.Models;

namespace NuKeeper.BitBucketLocal
{
    public class BitbucketLocalRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public BitbucketLocalRestClient(HttpClient client, INuKeeperLogger logger, string username, string appPassword)
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
            var response = await _client.GetAsync(url).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode) return default;

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(responseBody);
        }


        public async Task<IEnumerable<Repository>> GetProjects()
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Repository>>("projects?limit=999").ConfigureAwait(false);
            return response.Values;
        }


        public async Task<IEnumerable<Repository>> GetGitRepositories(string projectName)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Repository>>($"projects/{projectName}/repos?limit=999").ConfigureAwait(false);
            return response.Values;
        }

        public async Task<IEnumerable<string>> GetGitRepositoryFileNames(string projectName, string repositoryName)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<string>>($"projects/{projectName}/repos/{repositoryName}/files?limit=9999").ConfigureAwait(false);
            return response.Values;
        }

        public async Task<IEnumerable<Branch>> GetGitRepositoryBranches(string projectName, string repositoryName)
        {
            var response = await GetResourceOrEmpty<IteratorBasedPage<Branch>>($"projects/{projectName}/repos/{repositoryName}/branches").ConfigureAwait(false);
            return  response.Values;
        }

        public async Task<PullRequest> CreatePullRequest(PullRequest pullReq, string projectName, string repositoryName)
        {
            var requestJson = JsonConvert.SerializeObject(pullReq, Formatting.None, new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            var requestBody = new StringContent(requestJson, Encoding.UTF8, "application/json");
            
            var response = await _client.PostAsync($@"projects/{projectName}/repos/{repositoryName}/pull-requests", requestBody).ConfigureAwait(false);

            var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var resource = JsonConvert.DeserializeObject<PullRequest>(result);
            return resource;
        }
    }


}
