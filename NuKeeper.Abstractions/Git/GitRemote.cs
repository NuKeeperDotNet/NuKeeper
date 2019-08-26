using System;

namespace NuKeeper.Abstractions.Git
{
    public class GitRemote
    {
        public string Name { get; set; }
        public Uri Url { get; set; }
    }
}
