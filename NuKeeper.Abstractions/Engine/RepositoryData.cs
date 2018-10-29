using System;

namespace NuKeeper.Abstractions.Engine
{
    public class RepositoryData : IRepositoryData
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
