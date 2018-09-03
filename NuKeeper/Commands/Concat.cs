using System.Linq;

namespace NuKeeper.Commands
{
    public static class Concat
    {
        public static string FirstValue(params string[] values)
        {
            return values.FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));
        }
    }
}
