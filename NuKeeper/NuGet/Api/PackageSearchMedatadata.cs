using System;
using NuGet.Packaging.Core;

namespace NuKeeper.NuGet.Api
{
    public class PackageSearchMedatadata
    {
        public PackageSearchMedatadata(
            PackageIdentity identity,
            string source,
            DateTimeOffset? published)
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
        }

        public PackageIdentity Identity { get; }
        public string Source { get; }
        public DateTimeOffset? Published { get; }

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
