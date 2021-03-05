using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NuKeeper.Abstractions.CollaborationModels;

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
            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            IEnumerable<string> dotNetCodeExtensions = new ReadOnlyCollection<string>(new List<string>(){ ".sln", ".csproj", ".fsproj", ".vbproj" });
            const string dotNetCodeTerms = "\"packages.config\" OR \".csproj\" OR \".fsproj\" OR \".vbproj\"";

            var repos = new List<SearchRepo>
            {
                new SearchRepo(repository.RepositoryOwner, repository.RepositoryName)
            };

            var searchCodeRequest = new SearchCodeRequest(repos, dotNetCodeTerms, dotNetCodeExtensions)
            {
                PerPage = 1
            };

            try
            {
                var result = await _collaborationFactory.CollaborationPlatform.Search(searchCodeRequest);
                if (result.TotalCount <= 0)
                {
                    _logger.Detailed(
                        $"Repository {repository.RepositoryOwner}/{repository.RepositoryName} contains no .NET code on the default branch, skipping.");
                    return false;
                }

                return true;
            }
#pragma warning disable CA1031
            catch (Exception ex)
#pragma warning restore CA1031
            {
                _logger.Error("Repository search failed.", ex);
            }

            return true;

        }
    }
}
