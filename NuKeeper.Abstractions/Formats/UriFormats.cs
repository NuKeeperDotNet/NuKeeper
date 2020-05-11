using System;
using System.IO;
using System.Threading.Tasks;
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

        public static async Task<Uri> GetRemoteUriFromLocalRepo(this Uri repositoryUri, IGitDiscoveryDriver discoveryDriver, string shouldMatchTo)
        {
            if (discoveryDriver == null)
            {
                throw new ArgumentNullException(nameof(discoveryDriver));
            }

            if (await discoveryDriver.IsGitRepo(repositoryUri))
            {
                // Check the origin remotes
                var origin = await discoveryDriver.GetRemoteForPlatform(repositoryUri, shouldMatchTo);

                if (origin != null)
                {
                    return origin.Url;
                }
            }

            return repositoryUri;
        }

        public static Uri ToUri(this string repositoryString)
        {
            var repositoryUri = new Uri(repositoryString, UriKind.RelativeOrAbsolute);
            if (repositoryUri.IsAbsoluteUri)
            {
                return repositoryUri;
            }

            var absoluteUri = Path.Combine(Environment.CurrentDirectory, repositoryUri.OriginalString);
            if (!Directory.Exists(absoluteUri))
            {
                throw new NuKeeperException($"Local uri doesn't exist: {absoluteUri}");
            }

            return new Uri(absoluteUri);
        }
    }
}
