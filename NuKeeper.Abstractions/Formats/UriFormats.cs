using System;
using NuKeeper.Abstractions.Git;

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

        public static Uri GetRemoteUriFromLocalRepo(this Uri repositoryUri, IGitDiscoveryDriver discoveryDriver, string shouldMatchTo)
        {
            if (discoveryDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var origin = discoveryDriver.GetRemoteForPlatform(repositoryUri, shouldMatchTo);

                if (origin != null)
                {
                    return origin.Url;
                }
            }
            
            return repositoryUri;
        }
    }
}
