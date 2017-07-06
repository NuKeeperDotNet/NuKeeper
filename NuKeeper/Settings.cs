using System;

namespace NuKeeper
{
    public class Settings
    {
        public Uri GitUri { get; set; }

        public Uri GithubBaseUri { get; set; }
        public string GithubToken { get; set; }

        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}