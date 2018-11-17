using System;
using System.Linq;
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

        public static void SetUriFromLocalRepo(this Uri repositoryUri, IGitDiscoveryDriver discoveryDriver)
        {
            if (discoveryDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var remotes = discoveryDriver.GetRemotes(repositoryUri);
                var origin = remotes.FirstOrDefault(a =>
                    a.Name.Equals("origin", StringComparison.OrdinalIgnoreCase));

                if (origin != null)
                    repositoryUri = origin.Url;
            }
            else
            {
                throw new NuKeeperException("No local git repository found");
            }
        }
    }
}
