using System;
using NuKeeper.Inspection.Formats;
using System.Collections.Generic;
using System.Linq;
using NuGet.Configuration;

namespace NuKeeper.Inspection.Sources
{
    public class NuGetSources
    {
        public NuGetSources(params string[] sources)
        {
            if (!sources.Any())
            {
                throw new ArgumentException(nameof(sources));
            }

            Items = sources
                .Select(s => new PackageSource(s))
                .ToList();
        }

        public NuGetSources(IEnumerable<PackageSource> sources)
        {
            if (sources == null)
            {
                throw new ArgumentNullException(nameof(sources));
            }

            var items = sources.ToList();

            if (!items.Any())
            {
                throw new ArgumentException(nameof(sources));
            }

            Items = items;
        }

        public static NuGetSources GlobalFeed => new NuGetSources(NuGetConstants.V3FeedUrl);

        public IReadOnlyCollection<PackageSource> Items { get; }

        public string CommandLine(string prefix)
        {
            return Items
                .Select(s => $"{prefix} {EscapePathIfLocal(s)}")
                .JoinWithSeparator(" ");
        }

        public override string ToString()
        {
            return Items.Select(s => s.SourceUri.ToString()).JoinWithCommas();
        }

        private static string EscapePathIfLocal(PackageSource source)
        {
            return source.IsLocal ? $"\"{source.Source}\"" : source.Source;
        }
    }
}
