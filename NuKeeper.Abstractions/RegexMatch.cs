using System.Text.RegularExpressions;

namespace NuKeeper.Abstractions
{
    public static class RegexMatch
    {
        public static bool IncludeExclude(string target, Regex include, Regex exclude)
            => IsIncluded(target, include) && !IsExcluded(target, exclude);

        private static bool IsIncluded(string target, Regex include)
            => include?.IsMatch(target) ?? true;

        private static bool IsExcluded(string target, Regex exclude)
            => exclude?.IsMatch(target) ?? false;
    }
}
