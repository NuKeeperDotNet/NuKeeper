using System.Collections.Generic;
using System.Linq;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public class PackageUpdateSelection : IPackageUpdateSelection
    {
        private readonly int _maxPullRequests;

        public PackageUpdateSelection(int maxPullRequests)
        {
            _maxPullRequests = maxPullRequests;
        }

        public List<PackageUpdateSet> SelectTargets(IEnumerable<PackageUpdateSet> potentialUpdates)
        {
            return potentialUpdates
                .OrderByDescending(Priority)
                .Take(_maxPullRequests)
                .ToList();
        }

        private int Priority(PackageUpdateSet update)
        {
            return update.CountCurrentVersions();
        }
    }
}
