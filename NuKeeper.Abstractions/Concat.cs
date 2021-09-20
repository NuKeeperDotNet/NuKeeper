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

        public static IReadOnlyCollection<T> FirstNonEmptyCollection<T>(params IReadOnlyCollection<T>[] values)
        {
            return values.FirstOrDefault(HasElements);
        }

        private static bool HasElements<T>(IEnumerable<T> strings)
        {
            return strings != null && strings.Any();
        }
    }
}
