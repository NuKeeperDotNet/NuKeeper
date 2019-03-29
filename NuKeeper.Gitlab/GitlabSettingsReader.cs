using System;
using System.Linq;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.Gitlab
{
    public class GitlabSettingsReader : ISettingsReader
    {
        private const string GitLabTokenEnvironmentVariableName = "NuKeeper_gitlab_token";
        private const string UrlPattern = "https://gitlab.com/{username}/{projectname}.git";

        public Platform Platform => Platform.GitLab;

        public bool CanRead(Uri repositoryUri)
        {
            if (repositoryUri == null)
                return false;

            return repositoryUri.Host.Contains("gitlab", StringComparison.OrdinalIgnoreCase);
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.PreferFork;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable(GitLabTokenEnvironmentVariableName);
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch = null)
        {
            if (repositoryUri == null)
            {
                throw new NuKeeperException(
                    $"The provided uri was is not in the correct format. Provided null and format should be {UrlPattern}");
            }

            // Assumption - url should look like https://gitlab.com/{username}/{projectname}.git";
            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                throw new NuKeeperException(
                    $"The provided uri was is not in the correct format. Provided {repositoryUri} and format should be {UrlPattern}");
            }

            var repoOwner = pathParts[0];
            var repoName = pathParts[1].Replace(".git", string.Empty);

            var uriBuilder = new UriBuilder(repositoryUri) { Path = "/api/v4/" };

            return new RepositorySettings
            {
                ApiUri = uriBuilder.Uri,
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = repoOwner,
                TargetBranch = targetBranch
            };
        }
    }
}
