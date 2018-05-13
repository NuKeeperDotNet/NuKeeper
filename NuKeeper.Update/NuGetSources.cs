using NuKeeper.Inspection.Formats;
using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Update
{
    public class NuGetSources
    {
        public NuGetSources(IEnumerable<string> sources)
        {
            Sources = sources.ToList();
        }

        public NuGetSources(params string[] sources)
        {
            Sources = sources.ToList();
        }

        public IReadOnlyCollection<string> Sources { get; }

        public string CommandLine(string prefix)
        {
            return Sources
                .Select(s => $"{prefix} {s}")
                .JoinWithSeparator(" ");
        }

        public override string ToString()
        {
            return Sources.JoinWithCommas();
        }
    }
}
