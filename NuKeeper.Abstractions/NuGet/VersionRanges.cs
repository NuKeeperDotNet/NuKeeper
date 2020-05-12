using NuGet.Versioning;

namespace NuKeeper.Abstractions.NuGet
{
    public static class VersionRanges
    {
        public static NuGetVersion SingleVersion(VersionRange range)
        {
            if (range == null || range.IsFloating || range.HasLowerAndUpperBounds)
            {
                return null;
            }

            return range.MinVersion;
        }
    }
}
