using System.Collections.Generic;

namespace NuKeeper.Abstractions
{
    public static class ListHelper
    {
        public static List<T> InList<T>(this T item)
        {
            return new List<T>
            {
                item
            };
        }
    }
}
