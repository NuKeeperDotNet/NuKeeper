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

            this.Sources = sources.Select(s => new PackageSource(s)).ToList();
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

            this.Sources = items;
        }

        private const string GlobalFeedUrl = "https://api.nuget.org/v3/index.json";

        public static readonly PackageSource GlobalPackageSource = new PackageSource(GlobalFeedUrl);

        public static NuGetSources GlobalFeed => new NuGetSources(GlobalFeedUrl);

        public IReadOnlyCollection<string> Items
        {
            get { return Sources.Select(s => s.SourceUri.ToString()).ToList(); }
        }

        public IReadOnlyCollection<PackageSource> Sources { get; }

        public string CommandLine(string prefix)
        {
            return Items
                .Select(s => $"{prefix} {s}")
                .JoinWithSeparator(" ");
        }

        public override string ToString()
        {
            return Items.JoinWithCommas();
        }
    }
}
