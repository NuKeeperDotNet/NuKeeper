using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using System;
using System.Linq;
using NuKeeper.Abstractions.Formats;
using NuKeeper.Abstractions.Git;

namespace NuKeeper.GitHub
{
    public class GitHubSettingsReader : ISettingsReader
    {
        private const string PlatformHost = "github";
        private readonly IGitDiscoveryDriver _gitDriver;

        public GitHubSettingsReader(IGitDiscoveryDriver gitDriver)
        {
            _gitDriver = gitDriver;
        }

        public Platform Platform => Platform.GitHub;

        public bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return false;
            }

            // Is the specified folder already a git repository?
            if (repositoryUri.IsFile)
            {
                repositoryUri = repositoryUri.GetRemoteUriFromLocalRepo(_gitDriver, PlatformHost);
            }

            return UriFormats.MatchesHost(repositoryUri, PlatformHost);
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.PreferFork;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable("NuKeeper_github_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri)
        {
            if (repositoryUri == null)
            {
                return null;
            }

            // general pattern is https://github.com/owner/reponame.git
            // from this we extract owner and repo name
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                return null;
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            return new RepositorySettings
            {
                ApiUri = new Uri("https://api.github.com/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner
            };
        }
    }
}
