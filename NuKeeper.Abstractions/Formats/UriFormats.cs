using System;

namespace NuKeeper.Abstractions.Formats
{
    public static class UriFormats
    {
        public static Uri EnsureTrailingSlash(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            var path = uri.ToString();

            if (path.EndsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return uri;
            }

            return new Uri(path + "/");
        }

    }
}
