using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.BitBucket
{
    public class BitbucketSettingsReader : ISettingsReader
    {
        public Platform Platform => Platform.Bitbucket;

        private static string Username { get; set; }
        
        public bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null || repositoryUri.Host != "api.bitbucket.org")
            {
                return false;
            }

            return true;
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            settings.Username = Username;
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.SingleRepositoryOnly;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable("NuKeeper_bitbucket_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }
            
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 4)
            {
                return null;
            }

            Username = pathParts[2];
            var repoName = pathParts[3];

            return new RepositorySettings
            {
                ApiUri = new Uri($"https://api.bitbucket.org/2.0"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = Username
            };
        }
    }
}
