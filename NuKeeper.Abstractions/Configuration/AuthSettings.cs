using System;

namespace NuKeeper.Abstractions.Configuration
{
    public class AuthSettings
    {
        /// <summary>
        /// Provide the credentials required to authenticate against a different repository providers.
        /// </summary>
        /// <param name="apiBase">Repository URLs e.g. https://dev.azure.com/ or https://api.github.com/ or https://developer.atlassian.com/bitbucket/api/2/reference/</param>
        /// <param name="token">Personal Access Tokens or Client Secret. The GitHub client with use anonymous authentication it the token is blank. I.e. public repositories</param>
        /// <param name="username"></param>
        /// <remarks>See <seealso cref="NuKeeper.Abstractions.CollaborationPlatform.CollaborationPlatformSettings"/> for a similar model.</remarks>
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
