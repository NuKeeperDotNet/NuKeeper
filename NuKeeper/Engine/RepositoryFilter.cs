using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.DTOs;
using NuKeeper.Abstractions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            if (repository.IsLocalRepo)
            {
                // if local repo, then we already have the source so we can do a directory scan
                var ext = new List<string> { ".csproj", ".fsproj", ".vbproj" };
                var files = Directory.GetFiles(repository.WorkingFolder.AbsolutePath, "*.*", SearchOption.AllDirectories)
                    .Where(s => ext.Contains(Path.GetExtension(s)) || Path.GetFileName(s).Equals( "packages.config",StringComparison.OrdinalIgnoreCase));
                 return files.Any();
            }
            else
            {
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

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.Error("Repository search failed.", ex);
                    return false;
                }
            }

        }
    }
}
