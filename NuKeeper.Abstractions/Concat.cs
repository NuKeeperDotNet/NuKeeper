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

        public static IReadOnlyCollection<string> FirstPopulatedList(params List<string>[] lists)
        {
            return lists.FirstOrDefault(HasElements);
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
