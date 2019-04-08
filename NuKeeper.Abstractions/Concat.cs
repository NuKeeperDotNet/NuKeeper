using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Abstractions
{
    public static class Concat
    {
        public static string FirstValue(params string[] values)
        {
            return values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }

        public static T FirstValue<T>(params T?[] values) where T : struct
        {
            return values.FirstOrDefault(i => i.HasValue) ?? default;
        }

        public static IReadOnlyCollection<string> FirstPopulatedList(List<string> list1, List<string> list2, List<string> list3)
        {
            if (HasElements(list1))
            {
                return list1;
            }

            if (HasElements(list2))
            {
                return list2;
            }

            if (HasElements(list3))
            {
                return list3;
            }

            return null;
        }

        private static bool HasElements(List<string> strings)
        {
            if (strings == null)
            {
                return false;
            }
            return strings.Count > 0;
        }
    }
}
