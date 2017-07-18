using System.Collections.Generic;
using System.Linq;
using NuKeeper.Configuration;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly RepositoryModeSettings _settings;

        public PackageUpdateSelection(RepositoryModeSettings settings)
        {
            _settings = settings;
        }

        public List<PackageUpdateSet> SelectTargets(IEnumerable<PackageUpdateSet> potentialUpdates)
        {
            return potentialUpdates
                .OrderByDescending(Priority)
                .Take(_settings.MaxPullRequestsPerRepository)
                .ToList();
        }

        private int Priority(PackageUpdateSet update)
        {
            return update.CountCurrentVersions();
        }
    }
}
