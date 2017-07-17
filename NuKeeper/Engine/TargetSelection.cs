using System.Collections.Generic;
using System.Linq;
using NuKeeper.Configuration;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class TargetSelection
    {
        private readonly RepositoryModeSettings _settings;

        public TargetSelection(RepositoryModeSettings settings)
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
