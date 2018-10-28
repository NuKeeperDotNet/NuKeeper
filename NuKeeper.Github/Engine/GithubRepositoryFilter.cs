using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Engine;
using NuKeeper.Github.Mappings;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.Github.Engine
{
    public class GithubRepositoryFilter : IRepositoryFilter
    {
        private readonly IClient _client;
        private readonly INuKeeperLogger _logger;

        public GithubRepositoryFilter(IClient client, INuKeeperLogger logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task<bool> ContainsDotNetProjects(IRepositorySettings repository)
        {
            var searchCodeRequest = new SearchCodeRequest("\"packages.config\" OR \".csproj\" OR \".fsproj\" OR \".vbproj\"")
            {
                Repos = new RepositoryCollection { { repository.Owner, repository.RepositoryName } },
                In = new[] { Octokit.CodeInQualifier.Path },
                PerPage = 1
            };
            var request = new GithubSearchCodeRequest(searchCodeRequest);

            try
            {
                var result = await _client.Search(request);
                if (result.TotalCount <= 0)
                {
                    _logger.Detailed(
                        $"Repository {repository.Owner}/{repository.RepositoryName} contains no .NET code on the default branch, skipping.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("GitHub Repository search failed.", ex);
            }

            return true;
        }
    }
}
