using System;

namespace NuKeeper.Configuration
{
    public class RepositoryModeSettings
    {
        public Uri GithubUri { get; set; }

        public Uri GithubBaseUri { get; set; }
        public string GithubToken { get; set; }

        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}