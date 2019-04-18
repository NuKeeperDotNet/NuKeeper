using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.Gitea
{
    public class GiteaSettingsReader : ISettingsReader
    {
        private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
        private const string GiteaTokenEnvironmentVariableName = "NuKeeper_gitea_token";
        private const string UrlPattern = "https://yourgiteaserver/{owner}/{repo}.git";
        private const string ApiBaseAdress = "/api/v1/";

        public GiteaSettingsReader(IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider;
        }

        public Platform Platform => Platform.Gitea;

        public bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null)
                return false;

            try
            {
                var baseaddress = repositoryUri.OriginalString.Replace(repositoryUri.LocalPath, "");

                // There is no real identifier for gitea repos so try to get the gitea swagger json
                var client = new HttpClient()
                {
                    BaseAddress = new Uri(baseaddress)
                };

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = client.GetAsync($"swagger.v1.json").Result;
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Error during repo check\n\r{0}", ex.Message);
            }

            return false;
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            var envToken = _environmentVariablesProvider.GetEnvironmentVariable(GiteaTokenEnvironmentVariableName);

            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch = null)
        {
            if (repositoryUri == null)
            {
                throw new NuKeeperException(
                    $"The provided uri was is not in the correct format. Provided null and format should be {UrlPattern}");
            }

            // Assumption - url should look like https://yourgiteaUrl/{username}/{projectname}.git";
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                throw new NuKeeperException(
                    $"The provided uri was is not in the correct format. Provided {repositoryUri} and format should be {UrlPattern}");
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            // [ Base URL: /api/v1 ] https://try.gitea.io/api/swagger#
            var uriBuilder = new UriBuilder(repositoryUri) { Path = ApiBaseAdress };

            return new RepositorySettings
            {
                ApiUri = uriBuilder.Uri,
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                RemoteInfo = targetBranch == null
                    ? null
                    : new RemoteInfo { BranchName = targetBranch }
            };
        }
    }
}
