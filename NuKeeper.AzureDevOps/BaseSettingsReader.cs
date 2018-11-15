using System;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.AzureDevOps
{
    public abstract class BaseSettingsReader : ISettingsReader
    {
        public Platform Platform => Platform.AzureDevOps;

        public abstract bool CanRead(Uri repositoryUri);

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            UpdateTokenSettings(settings);
            settings.ForkMode = settings.ForkMode ?? ForkMode.SingleRepositoryOnly;
        }

        private static void UpdateTokenSettings(CollaborationPlatformSettings settings)
        {
            var envToken = Environment.GetEnvironmentVariable("NuKeeper_azure_devops_token");
            settings.Token = Concat.FirstValue(envToken, settings.Token);
        }

        public abstract RepositorySettings RepositorySettings(Uri repositoryUri);
    }
}
