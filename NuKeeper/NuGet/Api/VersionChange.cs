using System;
using NuGet.Versioning;

namespace NuKeeper.NuGet.Api
{
    public static class VersionChangeFilter
    {
        public static Func<NuGetVersion, NuGetVersion, bool> FilterFor(VersionChange allowedChange)
        {
            switch (allowedChange)
            {
                case VersionChange.Major:
                    return (v1, v2) => true;

                case VersionChange.Minor:
                    return (v1, v2) => v1.Major == v2.Major;

                case VersionChange.Patch:
                    return (v1, v2) => (v1.Major == v2.Major) && (v1.Minor == v2.Minor);

                case VersionChange.None:
                    return (v1, v2) => (v1 == v2);

                default:
                    throw new Exception($"Unknown version change {allowedChange}");
            }
        }
    }

    public enum VersionChange
    {
        None = 0,
        Patch,
        Minor,
        Major
    };
}