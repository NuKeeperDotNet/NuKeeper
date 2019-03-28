using System;
using NuKeeper.Abstractions.CollaborationPlatform;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Gitlab
{
    public class GitlabSettingsReader: ISettingsReader
    {
        public Platform Platform => Platform.GitLab;
        
        public bool CanRead(Uri repositoryUri)
        {
            throw new NotImplementedException();
        }

        public RepositorySettings RepositorySettings(Uri repositoryUri, string targetBranch = null)
        {
            throw new NotImplementedException();
        }

        public void UpdateCollaborationPlatformSettings(CollaborationPlatformSettings settings)
        {
            throw new NotImplementedException();
        }
    }
}
