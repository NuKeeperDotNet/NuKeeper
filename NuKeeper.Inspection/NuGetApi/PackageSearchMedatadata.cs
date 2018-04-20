using System;
using System.Collections.Generic;
using System.Linq;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.Formats;

namespace NuKeeper.Inspection.NuGetApi
{
    public class PackageSearchMedatadata
    {
        public PackageSearchMedatadata(
            PackageIdentity identity,
            string source,
            DateTimeOffset? published,
            IEnumerable<PackageDependency> dependencies)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }

            if (string.IsNullOrWhiteSpace(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            Identity = identity;
            Source = source;
            Published = published;

            Dependencies = dependencies?.ToList() ?? new List<PackageDependency>();
        }

        public PackageIdentity Identity { get; }
        public string Source { get; }
        public DateTimeOffset? Published { get; }

        public IReadOnlyCollection<PackageDependency> Dependencies { get; }

        public override string ToString()
        {
            if (Published.HasValue)
            {
                return $"{Identity} from {Source}, published at {DateFormat.AsUtcIso8601(Published)}";
            }

            return $"{Identity} from {Source}, no published date";
        }
    }
}
