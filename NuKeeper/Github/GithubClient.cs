using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NuKeeper.Configuration;

namespace NuKeeper.Github
{
    public class GithubClient : IGithub
    {
        private readonly Settings _settings;
        private readonly GithubRequestBuilder _requestBuilder;
        private Uri _next;
        private HttpClient _client;

        public GithubClient(Settings settings)
        {
            _settings = settings;
            _requestBuilder = new GithubRequestBuilder(settings.GithubToken);
            _client = new HttpClient();
        }

        private async Task<HttpResponseMessage> PostAsync<T>(string path, T content)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var uri = new Uri(_settings.GithubApiBase, path);
            var requestUri = _requestBuilder.AddSecretToken(uri);
            var request = _requestBuilder.ConstructRequestMessage(requestUri, HttpMethod.Post);
            request.Content = ToJsonContent(content);

            return await _client.SendAsync(request);
        }

        private HttpContent ToJsonContent<T>(T content)
        {
            var requestBody = GithubSerialization.SerializeObject(content);
            const string gitHubApiJsonType = "application/vnd.github.v3.text+json";
            return new StringContent(requestBody, Encoding.UTF8, gitHubApiJsonType);
        }

        public async Task<OpenPullRequestResult> OpenPullRequest(OpenPullRequestRequest request)
        {
            var path = $"/repos/{request.RepositoryOwner}/{request.RepositoryName}/pulls";
            var response = await PostAsync(path, request.Data);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Post failed: {response.StatusCode} {content}");
            }

            return new OpenPullRequestResult(response);
        }

        public async Task<IEnumerable<GithubRepository>> GetRepositoriesForOrganisation(string organisationName)
        {
            return await GetListOf<GithubRepository>($"orgs/{organisationName}/repos");
        }

        public async Task<IEnumerable<T1>> GetListOf<T1>(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            var uri = new Uri(_settings.GithubApiBase, path);

            _next = null;
            var resultsStorage = new List<T1>();
            do
            {
                var requestUri = _next ?? _requestBuilder.AddSecretToken(uri);

                var request = _requestBuilder.ConstructRequestMessage(requestUri, HttpMethod.Get);

                var response = await _client.SendAsync(request);

                var content = await response.Content.ReadAsStringAsync();

                var results = GithubSerialization.DeserializeObject<IEnumerable<T1>>(content);

                _next = GithubResponseParser.GetNextUri(response.Headers);

                resultsStorage.AddRange(results);
            } while (_next != null);

            return resultsStorage;
        }
    }
}