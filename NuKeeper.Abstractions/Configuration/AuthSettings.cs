using System;

namespace NuKeeper.Abstractions.Configuration
{
    public class AuthSettings
    {
        public AuthSettings(Uri apiBase, string token)
        {
            ApiBase = apiBase;
            Token = token;
        }

        public Uri ApiBase { get; }
        public string Token { get; }
        public string AzureDevOpsOrganisation { get; set; }
    }
}
