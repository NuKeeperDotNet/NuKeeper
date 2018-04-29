using System.Collections.Generic;
using System.Linq;
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

    public class NodeData
    {
        public NodeData(PackageUpdateSet set, IEnumerable<PackageDependency> deps)
        {
            PackageUpdateSet = set;
            Dependencies = new List<PackageDependency>(deps);
            Mark = Mark.None;
        }
        public PackageUpdateSet PackageUpdateSet { get; }

        public IReadOnlyCollection<PackageDependency> Dependencies { get; }

        public Mark Mark { get; set; }
    }

    /// <summary>
    /// https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
    /// </summary>
    public class DependencyOrder
    {
        private List<PackageUpdateSet> _output;
        private List<NodeData> _data;

        private bool _cycleFound;

        public IList<PackageUpdateSet> Sort(IList<PackageUpdateSet> priorityOrder)
        {
            if (priorityOrder.Count < 2)
            {
                return priorityOrder;
            }

            _output = new List<PackageUpdateSet>();
            _cycleFound = false;

            _data = priorityOrder
                .Select(p => MakeNode(p, priorityOrder))
                .ToList();

            foreach (var item in _data)
            {
                if (item.Mark == Mark.None)
                {
                    Visit(item);
                }
            }

            if (_cycleFound)
            {
                return priorityOrder;
            }

            return _output;
        }

        private void Visit(NodeData item)
        {
            if (item.Mark == Mark.Permanent)
            {
                return;
            }

            if (item.Mark == Mark.Temporary)
            {
                // cycle!
                _cycleFound = true;
                return;
            }

            item.Mark = Mark.Temporary;
            foreach (var dep in item.Dependencies)
            {
                var nodeForDep = _data.First(i => i.PackageUpdateSet.SelectedId == dep.Id);
                Visit(nodeForDep);
            }

            item.Mark = Mark.Permanent;
            _output.Add(item.PackageUpdateSet);
        }

        private static NodeData MakeNode(PackageUpdateSet set, IList<PackageUpdateSet> all)
        {
            var relevantDeps = set.Selected.Dependencies
                .Where(dep => all.Any(a => a.SelectedId == dep.Id));
            return new NodeData(set, relevantDeps);
        }
    }
}
