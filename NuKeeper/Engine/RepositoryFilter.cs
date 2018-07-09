using System;
using System.Threading.Tasks;
using NuKeeper.Configuration;
using NuKeeper.Github;
using NuKeeper.Inspection.Logging;
using Octokit;

namespace NuKeeper.Engine
{
    public class RepositoryFilter : IRepositoryFilter
    {
        private readonly IGithub _githubClient;
        private readonly INuKeeperLogger _logger;

        public RepositoryFilter(IGithub githubClient, INuKeeperLogger logger)
        {
            _githubClient = githubClient;
            _logger = logger;
        }

        public async Task<bool> ShouldSkip(RepositorySettings repository)
        {
            var request = new SearchCodeRequest("\"packages.config\" OR \".csproj\" OR \".fsproj\" OR \".vbproj\"")
            {
                Repos = new RepositoryCollection {{repository.RepositoryOwner, repository.RepositoryName}},
                In = new []{CodeInQualifier.Path},
                PerPage = 1
            };
            try
            {
                var result = await _githubClient.Search(request);
                if (result.TotalCount <= 0)
                {
                    _logger.Verbose(
                        $"Repository {repository.RepositoryOwner}/{repository.RepositoryName} contains no .NET code on the default branch, skipping.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("GitHub Repository search failed.", ex);
            }

            return false;
        }
    }
}
