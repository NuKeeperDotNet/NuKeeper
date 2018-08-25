using System.Collections.Generic;

namespace NuKeeper.Inspection.Sort
{
    public enum Mark
    {
        None,
        Temporary,
        Permanent
    }

    public class SortItemData<T>
    {
        public SortItemData(T item, IEnumerable<T> dependencies)
        {
            Item = item;
            Dependencies = new List<T>(dependencies);
            Mark = Mark.None;
        }
        public T Item { get; }

        public IReadOnlyCollection<T> Dependencies { get; }

        public Mark Mark { get; set; }
    }
}
