using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Commands
{
    public static class Concat
    {
        public static string FirstValue(params string[] values)
        {
            return values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }

        public static IReadOnlyCollection<string> FirstPopulatedList(string[] list1, string[] list2, string[] list3)
        {
            return FirstPopulatedArray(list1, list2, list3)
                .ToList();
        }

        private static string[] FirstPopulatedArray(string[] list1, string[] list2, string[] list3)
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

            return new string[0];
        }

        private static bool HasElements(string[] strings)
        {
            if (strings == null)
            {
                return false;
            }
            return strings.Length > 0;
        }
    }
}
