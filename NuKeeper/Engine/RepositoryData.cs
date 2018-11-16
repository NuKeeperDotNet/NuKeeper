using System;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Engine
{
    public class RepositoryData
    {
        public RepositoryData(ForkData pull, ForkData push)
        {
            Pull = pull ?? throw new ArgumentNullException(nameof(pull));
            Push = push ?? throw new ArgumentNullException(nameof(push));
            Remote = "nukeeper_push";
        }

        public ForkData Pull { get; }
        public ForkData Push { get; }
        public string DefaultBranch { get; set; }
        public string Remote { get; set; }
    }
}
