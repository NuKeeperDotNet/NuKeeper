using System;
using NuKeeper.Inspection.Formats;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Inspection.Sources
{
    public class NuGetSources
    {
        public NuGetSources(IEnumerable<string> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            Items = items.ToList();
        }

        public NuGetSources(params string[] items)
        {
            Items = items.ToList();
        }

        public static string GlobalFeedUrl = "https://api.nuget.org/v3/index.json";

        public static NuGetSources GlobalFeed => new NuGetSources(GlobalFeedUrl);

        public IReadOnlyCollection<string> Items { get; }

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
