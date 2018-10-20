using System;

namespace NuKeeper.AzureDevOps
{
    public class AzureDevopsAuthSettings
    {
        public AzureDevopsAuthSettings(Uri apiBase, string token)
        {
            ApiBase = apiBase;
            Token = token;
        }

        public Uri ApiBase { get; }
        public string Token { get; }
    }
}
