using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Versioning;

namespace NuKeeper.Inspection.NuGetApi
{
    public static class VersionChanges
    {
        public static PackageLookupResult MakeVersions(
            NuGetVersion current,
            IEnumerable<PackageSearchMedatadata> candiateVersions,
            VersionChange allowedChange)
        {
            var orderedCandidates = candiateVersions
                .OrderByDescending(p => p.Identity.Version)
                .ToList();

            var major = FirstMatch(orderedCandidates, current, VersionChange.Major);
            var minor = FirstMatch(orderedCandidates, current, VersionChange.Minor);
            var patch = FirstMatch(orderedCandidates, current, VersionChange.Patch);
            var release = FirstMatch(orderedCandidates, current, VersionChange.Release);
            return new PackageLookupResult(allowedChange, major, minor, patch, release);
        }

        private static PackageSearchMedatadata FirstMatch(
            IList<PackageSearchMedatadata> candidates,
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

                case VersionChange.Release:
                    if (string.IsNullOrEmpty(v1.Release))
                    {
                        return (v1.Major == v2.Major) && (v1.Minor == v2.Minor);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(v2.Release))
                        {
                            return (v1.Major == v2.Major) && (v1.Minor == v2.Minor);
                        }
                        else
                        {
                            return (v1.Major == v2.Major) && (v1.Minor == v2.Minor) && (v1.Patch == v2.Patch);
                        }
                    }

                case VersionChange.None:
                    return (v1 == v2);

                default:
                    throw new Exception($"Unknown version change {allowedChange}");
            }
        }

    }
}
