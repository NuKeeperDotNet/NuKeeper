using System;
using NuKeeper.Abstractions.CollaborationModels;

namespace NuKeeper.Abstractions.Configuration
{
    public class RepositorySettings
    {
        public RepositorySettings()
        {
        }

        public RepositorySettings(Repository repository)
        {
            RepositoryUri = repository.HtmlUrl;
            RepositoryOwner = repository.Owner.Login;
            RepositoryName = repository.Name;
        }

        public Uri RepositoryUri { get; set; }

        public string RepositoryOwner { get; set; }

        public string RepositoryName { get; set; }
        
        public Uri ApiUri { get; set; }

        public bool IsLocalRepo => RemoteInfo?.LocalRepositoryUri != null;

        public RemoteInfo RemoteInfo { get; set; }
    }
}
