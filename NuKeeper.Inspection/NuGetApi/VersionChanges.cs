using NuGet.Versioning;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGetApi;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Inspection.NuGetApi
{
    public static class VersionChanges
    {
        public static PackageLookupResult MakeVersions(
            NuGetVersion current,
            IEnumerable<PackageSearchMetadata> candidateVersions,
            VersionChange allowedChange)
        {
            var orderedCandidates = candidateVersions
                .OrderByDescending(p => p.Identity.Version)
                .ToList();

            var major = FirstMatch(orderedCandidates, current, VersionChange.Major);
            var minor = FirstMatch(orderedCandidates, current, VersionChange.Minor);
            var patch = FirstMatch(orderedCandidates, current, VersionChange.Patch);
            return new PackageLookupResult(allowedChange, major, minor, patch);
        }

        private static PackageSearchMetadata FirstMatch(
            IList<PackageSearchMetadata> candidates,
            NuGetVersion current,
            VersionChange allowedChange)
        {
            return candidates.FirstOrDefault(p => Filter(current, p.Identity.Version, allowedChange));
        }

        private static bool Filter(NuGetVersion v1, NuGetVersion v2, VersionChange allowedChange)
        {
            switch (allowedChange)
            {
                case VersionChange.Major:
                    return true;

                case VersionChange.Minor:
                    return v1.Major == v2.Major;

                case VersionChange.Patch:
                    return (v1.Major == v2.Major) && (v1.Minor == v2.Minor);

                case VersionChange.None:
                    return (v1 == v2);

                default:
                    throw new NuKeeperException($"Unknown version change {allowedChange}");
            }
        }
    }
}
