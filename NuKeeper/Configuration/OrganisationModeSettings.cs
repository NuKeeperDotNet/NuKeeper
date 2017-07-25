using System;

namespace NuKeeper.Configuration
{
    public class OrganisationModeSettings
    {
        public Uri GithubApiBase { get; set; }
        public string OrganisationName { get; set; }
        public string GithubToken { get; set; }
        public int MaxPullRequestsPerRepository { get; set;  }
    }
}