using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Gitea
{
    public class GiteaSettingsReader : ISettingsReader
    {
        private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;
        private const string GiteaTokenEnvironmentVariableName = "NuKeeper_gitea_token";
        private const string UrlPattern = "https://yourgiteaserver/{owner}/{repo}.git";
        private const string ApiBaseAdress = "api/v1/";

        public GiteaSettingsReader(IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider;
        }

        public Platform Platform => Platform.Gitea;

        public async Task<bool> CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null || repositoryUri.Segments == null || repositoryUri.Segments.Length < 3)
                return false;

            try
            {
                // There is no real identifier for gitea repos so try to get the gitea swagger json
                var client = new HttpClient()
                {
                    BaseAddress = GetBaseAddress(repositoryUri)
                };

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.GetAsync($"swagger.v1.json");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("No valid Gitea repo during repo check\n\r{0}", ex.Message);
            }

            return false;
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            var envToken = _environmentVariablesProvider.GetEnvironmentVariable(GiteaTokenEnvironmentVariableName);

            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public Task<RepositorySettings> RepositorySettings(Uri repositoryUri, string targetBranch = null)
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

            if (pathParts.Count < 2)
            {
                throw new NuKeeperException(
                    $"The provided uri was is not in the correct format. Provided {repositoryUri} and format should be {UrlPattern}");
            }

            var repoOwner = pathParts[pathParts.Count - 2];
            var repoName = pathParts[pathParts.Count - 1].Replace(".git", string.Empty);

            // [ Base URL: /api/v1 ] https://try.gitea.io/api/swagger#
            var baseAddress = GetBaseAddress(repositoryUri);
            var apiUri = new Uri(baseAddress, ApiBaseAdress);

            return Task.FromResult(new RepositorySettings
            {
                ApiUri = apiUri,
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                RemoteInfo = targetBranch == null
                    ? null
                    : new RemoteInfo { BranchName = targetBranch }
            });
        }

        private Uri GetBaseAddress(Uri repoUri)
        {
            var newSegments = repoUri.Segments.Take(repoUri.Segments.Length - 2).ToArray();
            var ub = new UriBuilder(repoUri);
            ub.Path = string.Concat(newSegments);

            return ub.Uri;
        }
    }
}
