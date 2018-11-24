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

namespace NuKeeper.AzureDevOps
{
    public class AzureDevOpsRestClient
    {
        private readonly HttpClient _client;
        private readonly INuKeeperLogger _logger;

        public AzureDevOpsRestClient(HttpClient client, INuKeeperLogger logger, string personalAccessToken)
        {
            _client = client;
            _logger = logger;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"{string.Empty}:{personalAccessToken}")));
        }

        private async Task<T> GetResourceOrEmpty<T>(string url, [CallerMemberName] string caller = null)
        {
            string msg;

            var fullUrl = BuildAzureDevOpsUri(url);

            _logger.Detailed($"{caller}: Requesting {fullUrl}");

            var response = await _client.GetAsync(fullUrl);
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
                        _logger.Error($"{caller}: Error {response.StatusCode}");
                        return default;
                }
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(responseBody);
            }
            catch (Exception)
            {
                msg = $"{caller}: Json exception - malformed PAT?";
                _logger.Error(msg);
                throw new NuKeeperException(msg);
            }
        }
        
        private static Uri BuildAzureDevOpsUri(string relativePath)
        {
            return new Uri($"{relativePath}?api-version=4.1", UriKind.Relative);
        }

        public async Task<IEnumerable<Project>> GetProjects()
        {
            var response = await GetResourceOrEmpty<ProjectResource>("/_apis/projects");
            return response?.value.AsEnumerable();
        }

        public async Task<IEnumerable<AzureRepository>> GetGitRepositories(string projectName)
        {
            var response = await GetResourceOrEmpty<GitRepositories>($"{projectName}/_apis/git/repositories");
            return response?.value.AsEnumerable();
        }

        public async Task<IEnumerable<GitRefs>> GetRepositoryRefs(string projectName,string repositoryId)
        {
            var response = await GetResourceOrEmpty<GitRefsResource>($"{projectName}/_apis/git/repositories/{repositoryId}/refs");
            return response?.value.AsEnumerable();
        }

        public async Task<PullRequest> CreatePullRequest(PRRequest request, string projectName, string azureRepositoryId)
        {
            var response = await _client.PostAsync(BuildAzureDevOpsUri(($"{projectName}/_apis/git/repositories/{azureRepositoryId}/pullrequests")), new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json"));
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
}
