using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NuGet.Protocol;

namespace NuKeeper.Github
{
    public class GithubClient : IGithub
    {
        private readonly Settings _settings;
        private readonly GithubRequestBuilder _requestBuilder;

        private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            ContractResolver = new LowercaseContractResolver()
        };


        public GithubClient(Settings settings)
        {
            _settings = settings;
            _requestBuilder = new GithubRequestBuilder(settings.GithubToken);
        }

        private async Task<HttpResponseMessage> PostAsync<T>(Uri uri, T content)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            var requestUri = _requestBuilder.AddSecretToken(uri);

            var requestBody = JsonConvert.SerializeObject(content, jsonSettings);

            var request = _requestBuilder.ConstructRequestMessage(requestUri, HttpMethod.Post);
            request.Content = new StringContent(requestBody, Encoding.UTF8, "application/vnd.github.v3.text+json");

            var client = new HttpClient();
            return await client.SendAsync(request);
        }

        public async Task<OpenPullRequestResult> OpenPullRequest(OpenPullRequestRequest request)
        {
            var uri = new Uri(_settings.GithubBaseUri, $"/repos/{request.RepositoryOwner}/{request.RepositoryName}/pulls");
            var response = await PostAsync(uri, request.Data);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"Post failed: {response.StatusCode} {content}");
            }

            return new OpenPullRequestResult(response);
        }
    }
}