using System;

namespace NuKeeper.GitHub
{
    public static class GithubUriHelpers
    {
        public static Uri Normalise(Uri value)
        {
            return Normalise(value.ToString());
        }

        public static Uri Normalise(string value)
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
