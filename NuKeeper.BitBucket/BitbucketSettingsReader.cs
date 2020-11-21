using System;
using System.Linq;
using System.Threading.Tasks;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.Formats;

namespace NuKeeper.BitBucket
{
    public class BitbucketSettingsReader : ISettingsReader
    {
        private readonly IEnvironmentVariablesProvider _environmentVariablesProvider;

        public BitbucketSettingsReader(IEnvironmentVariablesProvider environmentVariablesProvider)
        {
            _environmentVariablesProvider = environmentVariablesProvider;
        }

        public Platform Platform => Platform.Bitbucket;

        private string Username { get; set; }

        public Task<bool> CanRead(Uri repositoryUri)
        {
            return Task.FromResult(repositoryUri?.Host.Contains("bitbucket.org", StringComparison.OrdinalIgnoreCase) == true);
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            settings.Username = Username;
            var envToken = _environmentVariablesProvider.GetEnvironmentVariable("NuKeeper_bitbucket_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
            settings.ForkMode = settings.ForkMode ?? ForkMode.SingleRepositoryOnly;
        }

        public Task<RepositorySettings> RepositorySettings(Uri repositoryUri, bool setAutoMerge, string targetBranch = null)
        {
            if (repositoryUri == null)
            {
                return Task.FromResult<RepositorySettings>(null);
            }

            var path = repositoryUri.AbsolutePath;
            var pathParts = path.Split('/')
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (pathParts.Count != 2)
            {
                throw new NuKeeperException($"The provided uri was is not in the correct format. Provided {repositoryUri} and format should be https://username_@bitbucket.org/projectname/repositoryname.git");
            }

            if (string.IsNullOrWhiteSpace(repositoryUri.UserInfo))
            {
                Username = pathParts[0];
            }
            else
            {
                Username = repositoryUri.UserInfo.Split(':').First();
            }

            var repoName = pathParts[1];
            //Trim off any .git extension from repo name
            repoName = repoName.EndsWith(".git", StringComparison.InvariantCultureIgnoreCase) ?
                repoName.Substring(0, repoName.LastIndexOf(".git", StringComparison.InvariantCultureIgnoreCase))
                : repoName;
            var owner = pathParts[0];

            return Task.FromResult(new RepositorySettings
            {
                ApiUri = new Uri("https://api.bitbucket.org/2.0/"),
                RepositoryUri = repositoryUri,
                RepositoryName = repoName,
                RepositoryOwner = owner,
                RemoteInfo = targetBranch != null ? new RemoteInfo { BranchName = targetBranch } : null
            });
        }
    }
}
