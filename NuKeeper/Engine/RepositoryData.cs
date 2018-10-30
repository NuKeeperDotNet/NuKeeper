using System;
using NuKeeper.Abstractions.DTOs;

namespace NuKeeper.Engine
{
    public class RepositoryData
    {
        public RepositoryData(ForkData pull, ForkData push)
        {
            if (pull == null)
            {
                throw new ArgumentNullException(nameof(pull));
            }

            if (push == null)
            {
                throw new ArgumentNullException(nameof(push));
            }

            Pull = pull;
            Push = push;
        }

        public ForkData Pull { get; }
        public ForkData Push { get; }
        public string DefaultBranch { get; set; }
    }
}
