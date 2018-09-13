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

        public static IEnumerable<string> AllPopulated(IEnumerable<string> values, string value)
        {
            var result = new List<string>();
            if (values != null)
            {
                result.AddRange(values.Where(s => ! string.IsNullOrWhiteSpace(s)));
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                result.Add(value);
            }

            return result;
        }
    }
}
