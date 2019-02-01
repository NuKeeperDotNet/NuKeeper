using System;

namespace NuKeeper.GitHub
{
    public static class GithubHelpers
    {
        public static Uri GithubUri(Uri value)
        {
            return GithubUri(value.ToString());
        }

        public static Uri GithubUri(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (value.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 4);
            }

            if (value.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(0, value.Length - 1);
            }

            return new Uri(value, UriKind.Absolute);
        }
    }
}
