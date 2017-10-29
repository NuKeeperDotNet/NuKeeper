using System;

namespace NuKeeper.Configuration
{
    public class GithubAuthSettings
    {
        public GithubAuthSettings(Uri apiBase, string token)
        {
            ApiBase = apiBase;
            Token = token;
        }

        public Uri ApiBase { get; }
        public string Token { get; }
    }
}