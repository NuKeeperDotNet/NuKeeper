using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Inspection.Report
{
    public static class Age
    {
        public static TimeSpan Sum(IEnumerable<PackageUpdateSet> updates)
        {
            var now = DateTimeOffset.UtcNow;

            var sum = updates
                .Select(u => u.Selected.Published)
                .Where(p => p.HasValue)
                .Select(p => now.Subtract(p.Value))
                .Aggregate(TimeSpan.Zero, (t1, t2) => t1.Add(t2));

            return sum;
        }
    }
}
