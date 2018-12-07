using System;
using System.IO;
using NuGet.Common;
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
                    return origin.Url;
            }
            
            return repositoryUri;
        }

        public static Uri ToUri(this string test)
        {
            var calledFrom = Environment.CurrentDirectory;
            
            var repositoryUri = new Uri(test, UriKind.RelativeOrAbsolute);

            if (repositoryUri.IsAbsoluteUri) return repositoryUri;
            
            var absoluteUri = Path.Combine(calledFrom, repositoryUri.OriginalString);
            
            if (!Directory.Exists(absoluteUri))
                throw new NuKeeperException($"Local uri doesn't exist: {absoluteUri}");
            
            return new Uri(absoluteUri);
        }
    }
}
