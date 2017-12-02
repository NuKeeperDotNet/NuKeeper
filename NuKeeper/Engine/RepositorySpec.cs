using System;

namespace NuKeeper.Engine
{
    public class RepositorySpec
    {
        public RepositorySpec(ForkSpec pull, ForkSpec push)
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

        public ForkSpec Pull { get; }
        public ForkSpec Push { get; }
        public string DefaultBranch { get; set; }
    }
}