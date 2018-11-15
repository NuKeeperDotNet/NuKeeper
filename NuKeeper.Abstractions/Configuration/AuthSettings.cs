using System;

namespace NuKeeper.Abstractions.Configuration
{
    public class AuthSettings
    {
        public AuthSettings(Uri apiBase, string token, string username = null)
        {
            ApiBase = apiBase;
            Token = token;
            Username = username;
        }

        public Uri ApiBase { get; }
        public string Token { get; }
        public string Username { get; }
    }
}
