using System;
using System.Globalization;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.BitBucketLocal
{
    public class BitBucketLocalSettingsReader : ISettingsReader
    {
        public Platform Platform { get; } = Platform.BitbucketLocal;

        private string Username { get; set; }

        public bool CanRead(Uri repositoryUri)
        {
            return repositoryUri?.Host.Contains("bitbucket", StringComparison.OrdinalIgnoreCase) == true &&
                   repositoryUri.Host.Contains("bitbucket.org", StringComparison.OrdinalIgnoreCase) == false;
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            Username = Concat.FirstValue(repositoryUri.UserInfo, Environment.UserName);

            if (pathParts.Count < 2)
            {
                return null;
            }

            var repoName = pathParts[pathParts.Count - 1].ToLower(CultureInfo.CurrentCulture).Replace(".git", string.Empty);
            var project = pathParts[pathParts.Count - 2];

            return new RepositorySettings
            {
                ApiUri = new Uri($"{repositoryUri.Scheme}://{repositoryUri.Authority}"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = project
            };
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            settings.Username = Concat.FirstValue(Username, Environment.UserName);
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.SingleRepositoryOnly;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable("NuKeeper_bitbucketlocal_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }
    }
}


