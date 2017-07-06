using System;
using System.Net.Http;

namespace NuKeeper.Github
{
    public class GithubRequestBuilder
    {
        private const string AcceptHeader = "application/vnd.github.v3+json";
        private const string UserAgent = "NuKeeper/1.0.0.0";
        private readonly string _secretToken;

        public GithubRequestBuilder(string secretToken)
        {
            if (string.IsNullOrWhiteSpace(secretToken))
            {
                throw new ArgumentNullException(nameof(secretToken));
            }

            _secretToken = secretToken;
        }

        public Uri AddSecretToken(Uri uri)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            string queryString = uri.Query;

            var tokenQuery = $"access_token={_secretToken}";

            if (!queryString.StartsWith("?"))
            {
                queryString = $"?{queryString}";
            }
            else
            {
                tokenQuery = $"&{tokenQuery}";
            }

            queryString += tokenQuery;

            return new Uri(uri, queryString);
        }

        public HttpRequestMessage ConstructRequestMessage(Uri requestUri, HttpMethod method, string acceptHeader = null)
        {
            var request = new HttpRequestMessage(method, requestUri);
            request.Headers.Add("Accept", acceptHeader ?? AcceptHeader);
            request.Headers.Add("User-Agent", UserAgent);
            return request;
        }
    }
}