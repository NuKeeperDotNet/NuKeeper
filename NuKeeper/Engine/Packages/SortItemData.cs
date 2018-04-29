using System.Collections.Generic;
using NuGet.Packaging.Core;
using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.Engine.Packages
{
    public enum Mark
    {
        None,
        Temporary,
        Permanent
    }

    public class SortItemData
    {
        public SortItemData(PackageUpdateSet set, IEnumerable<PackageDependency> deps)
        {
            PackageUpdateSet = set;
            Dependencies = new List<PackageDependency>(deps);
            Mark = Mark.None;
        }
        public PackageUpdateSet PackageUpdateSet { get; }

        public IReadOnlyCollection<PackageDependency> Dependencies { get; }

        public Mark Mark { get; set; }

        public string PackageId => PackageUpdateSet.SelectedId;
    }
}
