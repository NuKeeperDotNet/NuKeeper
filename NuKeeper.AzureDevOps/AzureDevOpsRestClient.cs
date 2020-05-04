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

        private static Uri BuildAzureDevOpsUri(string relativePath, bool previewApi = false)
        {
            return previewApi
                ? new Uri($"{relativePath}?api-version=4.1-preview.1", UriKind.Relative)
                : new Uri($"{relativePath}?api-version=4.1", UriKind.Relative);
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

        public async Task<UserProfile> GetCurrentUserProfile(string projectName) => await GetResource<UserProfile>($"{projectName}/_apis/profile/profiles/me");
    }
}
