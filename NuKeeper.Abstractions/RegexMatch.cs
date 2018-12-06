using System.Text.RegularExpressions;

namespace NuKeeper.Abstractions
{
    public static class RegexMatch
    {
        public static bool IncludeExclude(string target, Regex include, Regex exclude)
        {
            return
                IsIncluded(target, include)
                && !IsExcluded(target, exclude);
        }

        private static bool IsIncluded(string target, Regex regex)
        {
            return regex == null || regex.IsMatch(target);
        }

        private static bool IsExcluded(string target, Regex regex)
        {
            return regex != null && regex.IsMatch(target);
        }
    }
}
