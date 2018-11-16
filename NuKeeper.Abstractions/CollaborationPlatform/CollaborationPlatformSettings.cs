using System;
using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationPlatform
{
    public class CollaborationPlatformSettings
    {
        public Uri BaseApiUrl { get; set; }
        public string Token { get; set; }
        public string Username { get; set; }
        public ForkMode? ForkMode { get; set; }
        
    }
}
