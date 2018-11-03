using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuKeeper.Engine
{
    public class RepositoryFilter : IRepositoryFilter
    {
        private readonly ICollaborationPlatform _collaborationPlatformClient;
        private readonly INuKeeperLogger _logger;

        public RepositoryFilter(ICollaborationPlatform collaborationPlatformClient, INuKeeperLogger logger)
        {
            _collaborationPlatformClient = collaborationPlatformClient;
            _logger = logger;
        }

        public async Task<bool> ContainsDotNetProjects(RepositorySettings repository)
        {
            var request = new SearchCodeRequest("\"packages.config\" OR \".csproj\" OR \".fsproj\" OR \".vbproj\"")
            {
                Repos = new List<(string owner, string name)> { (repository.RepositoryOwner, repository.RepositoryName) },
                PerPage = 1
            };
            try
            {
                var result = await _collaborationPlatformClient.Search(request);
                if (result.TotalCount <= 0)
                {
                    _logger.Detailed(
                        $"Repository {repository.RepositoryOwner}/{repository.RepositoryName} contains no .NET code on the default branch, skipping.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Repository search failed.", ex);
            }

            return true;
        }
    }
}
