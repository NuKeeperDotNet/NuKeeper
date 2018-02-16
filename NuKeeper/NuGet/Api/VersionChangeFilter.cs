using System;
using NuGet.Versioning;

namespace NuKeeper.NuGet.Api
{
    public static class VersionChangeFilter
    {
        public static bool Filter(NuGetVersion v1, NuGetVersion v2, VersionChange allowedChange)
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
                    throw new Exception($"Unknown version change {allowedChange}");
            }
        }

    }
}
