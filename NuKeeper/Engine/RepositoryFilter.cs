using System;
using System.Threading.Tasks;
using NuKeeper.Abstractions.Logging;
using NuKeeper.Configuration;
using NuKeeper.GitHub;
using Octokit;

namespace NuKeeper.Engine
{
    public class RepositoryFilter : IRepositoryFilter
    {
        private readonly IGitHub _gitHubClient;
        private readonly INuKeeperLogger _logger;

        public RepositoryFilter(IGitHub gitHubClient, INuKeeperLogger logger)
        {
            _gitHubClient = gitHubClient;
            _logger = logger;
        }

        public async Task<bool> ContainsDotNetProjects(RepositorySettings repository)
        {
            var request = new SearchCodeRequest("\"packages.config\" OR \".csproj\" OR \".fsproj\" OR \".vbproj\"")
            {
                Repos = new RepositoryCollection {{repository.RepositoryOwner, repository.RepositoryName}},
                In = new []{CodeInQualifier.Path},
                PerPage = 1
            };
            try
            {
                var result = await _gitHubClient.Search(request);
                if (result.TotalCount <= 0)
                {
                    _logger.Detailed(
                        $"Repository {repository.RepositoryOwner}/{repository.RepositoryName} contains no .NET code on the default branch, skipping.");
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
