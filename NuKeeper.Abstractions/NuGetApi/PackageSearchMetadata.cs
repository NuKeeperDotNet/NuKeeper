using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Abstractions.NuGetApi
{
    public class PackageSearchMetadata
    {
        public PackageSearchMetadata(
            PackageIdentity identity,
            PackageSource source,
            DateTimeOffset? published,
            IEnumerable<PackageDependency> dependencies)
        {
            Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Published = published;
            Dependencies = dependencies?.ToList() ?? new List<PackageDependency>();
        }

        public PackageIdentity Identity { get; }
        public PackageSource Source { get; }
        public DateTimeOffset? Published { get; }
        // TODO: how to get url for package hosted on private feeds or public feeds different from nuget?
        public Uri Url => GetPackageUrl();

        public IReadOnlyCollection<PackageDependency> Dependencies { get; }

        public override string ToString()
        {
            if (Published.HasValue)
            {
                return $"{Identity} from {Source}, published at {DateFormat.AsUtcIso8601(Published)}";
            }

            return $"{Identity} from {Source}, no published date";
        }

        private static bool IsNugetUrl(Uri sourceUrl)
        {
            return
                sourceUrl != null &&
                sourceUrl.ToString().StartsWith("https://api.nuget.org/", StringComparison.OrdinalIgnoreCase);
        }

        private Uri GetPackageUrl()
        {
            if (!IsNugetUrl(Source.SourceUri)) return null;

            return new Uri($"https://www.nuget.org/packages/{Identity.Id}/{Identity.Version}");
        }
    }
}
