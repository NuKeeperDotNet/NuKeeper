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
        private readonly ICollaborationFactory _collaborationFactory;
        private readonly INuKeeperLogger _logger;

        public RepositoryFilter(ICollaborationFactory collaborationFactory, INuKeeperLogger logger)
        {
            _collaborationFactory = collaborationFactory;
            _logger = logger;
        }

        public async Task<bool> ContainsDotNetProjects(RepositorySettings repository)
        {
            // TODO check if local repo, then we already have the source so we can do a directory scan
            var request = new SearchCodeRequest("\"packages.config\" OR \".csproj\" OR \".fsproj\" OR \".vbproj\"")
            {
                Repos = new List<(string owner, string name)> { (repository.RepositoryOwner, repository.RepositoryName) },
                PerPage = 1
            };
            try
            {
                var result = await _collaborationFactory.CollaborationPlatform.Search(request);
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
                // TODO fail the contains
            }

            return true;
        }
    }
}
