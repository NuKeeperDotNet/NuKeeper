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

        public static string[] FirstPopulatedList(string[] list1, string[] list2, string[] list3)
        {
            if ((list1?.Length ?? 0) > 0)
            {
                return list1;
            }

            if ((list2?.Length ?? 0) > 0)
            {
                return list2;
            }

            return list3;
        }
    }
}
