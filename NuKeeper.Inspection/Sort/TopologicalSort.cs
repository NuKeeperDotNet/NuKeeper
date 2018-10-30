using System;
using System.Collections.Generic;
using System.Linq;
using NuKeeper.Abstractions.Logging;

namespace NuKeeper.Inspection.Sort
{
    /// <summary>
    /// https://en.wikipedia.org/wiki/Topological_sorting#Depth-first_search
    /// </summary>
    public class TopologicalSort<T>
    {
        private readonly INuKeeperLogger _logger;
        private readonly Func<T, T, bool> _match;

        private readonly List<T> _sortedList = new List<T>();
        private List<SortItemData<T>> _data;
        private bool _cycleFound;

        public TopologicalSort(INuKeeperLogger logger, Func<T, T, bool> match)
        {
            _logger = logger;
            _match = match;
        }

        public IEnumerable<T> Sort(
            IReadOnlyCollection<SortItemData<T>> inputMap)
        {
            var inputItems = inputMap
                .Select(i => i.Item)
                .ToList();

            if (inputMap.Count < 2)
            {
                return inputItems;
            }

            if (!inputMap.Any(i => i.Dependencies.Any()))
            {
                _logger.Detailed("No dependencies between items, no need to sort on dependencies");
                return inputItems;
            }

            _data = inputMap.ToList();

            return DoSortVisits(inputItems);
        }

        private IReadOnlyCollection<T> DoSortVisits(IReadOnlyCollection<T>  input)
        { 
            foreach (var item in _data)
            {
                if (item.Mark == Mark.None)
                {
                    Visit(item);
                }
            }

            if (_cycleFound)
            {
                return input;
            }

            return _sortedList;
        }

        private void Visit(SortItemData<T> item)
        {
            if (_cycleFound)
            {
                return;
            }

            if (item.Mark == Mark.Permanent)
            {
                return;
            }

            if (item.Mark == Mark.Temporary)
            {
                _logger.Minimal($"Cannot sort by dependencies, cycle found at item {item}");
                _cycleFound = true;
                return;
            }

            item.Mark = Mark.Temporary;

            foreach (var dep in NodesDependedOn(item))
            {
                Visit(dep);
            }

            item.Mark = Mark.Permanent;
            _sortedList.Add(item.Item);
        }

        private IEnumerable<SortItemData<T>> NodesDependedOn(SortItemData<T> item)
        {
            return item.Dependencies
                .Select(dep => _data.FirstOrDefault(i => _match(i.Item, dep)))
                .Where(dep => dep != null);
        }
    }
}
