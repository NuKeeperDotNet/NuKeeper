using System;
using NuGet.Packaging.Core;
using NuGet.Versioning;

namespace NuKeeper.Abstractions.NuGet
{
    public class PackageVersionRange
    {
        public PackageVersionRange(string id, VersionRange version)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Should not be null or empty", nameof(id));
            }

            Id = id;
            Version = version ?? throw new ArgumentNullException(nameof(version));
        }

        public string Id { get; }
        public VersionRange Version { get; }

        public static PackageVersionRange Parse(string id, string version)
        {
            var success = VersionRange.TryParse(version, out VersionRange versionRange);
            if (!success)
            {
                return null;
            }

            return new PackageVersionRange(id, versionRange);
        }

        public PackageIdentity SingleVersionIdentity()
        {
            var version = VersionRanges.SingleVersion(Version);
            if (version == null)
            {
                return null;
            }

            return new PackageIdentity(Id, version);
        }
    }
}
