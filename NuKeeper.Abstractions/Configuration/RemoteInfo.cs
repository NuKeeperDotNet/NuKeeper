using System;

namespace NuKeeper.Abstractions.Configuration
{
    public class RemoteInfo
    {
        public Uri LocalRepositoryUri { get; set; }
        public Uri WorkingFolder { get; set; }
        public string BranchName { get; set; }
        public string RemoteName { get; set; }
    }
}
