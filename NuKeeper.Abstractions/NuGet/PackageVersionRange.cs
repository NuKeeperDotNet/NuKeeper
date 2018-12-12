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

            if (Version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            Id = id;
            Version = version;
        }

        public string Id { get; }
        public VersionRange Version { get; }

        public static PackageVersionRange Read(string id, string version)
        {
            var success = VersionRange.TryParse(version, out VersionRange versionRange);
            if (!success)
            {
                throw new ArgumentException($"Could not parse {version} as a version range", nameof(version));
            }

            return new PackageVersionRange(id, versionRange);
        }

        public NuGetVersion SingleVersion()
        {
            return VersionRanges.SingleVersion(Version);
        }

        public PackageIdentity Identity()
        {
            var version = SingleVersion();
            if (version == null)
            {
                return null;
            }

            return new PackageIdentity(Id, version);
        }
    }
}
