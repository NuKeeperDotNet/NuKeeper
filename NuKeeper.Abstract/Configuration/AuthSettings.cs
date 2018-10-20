using System;

namespace NuKeeper.Abstract.Configuration
{
    public class AuthSettings : IAuthSettings
    {
        public AuthSettings(Uri apiBase, string token)
        {
            ApiBase = apiBase;
            Token = token;
        }

        public Uri ApiBase { get; }
        public string Token { get; }
    }
}
