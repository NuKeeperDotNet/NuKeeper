using System;
using System.Collections.Generic;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using NuGet.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.Abstractions.NuGet
{
    public class NuGetSources
    {
        public NuGetSources(params string[] sources)
        {
            if (!sources.Any())
            {
                throw new ArgumentException("At least one source must be specified", nameof(sources));
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
                throw new ArgumentException("No package sources defined", nameof(sources));
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
            return source.IsLocal ? ArgumentEscaper.EscapeAndConcatenate(new[] { source.Source }) : source.Source;
        }
    }
}
