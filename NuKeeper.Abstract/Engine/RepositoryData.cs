using System;

namespace NuKeeper.Abstract.Engine
{
    public class RepositoryData : IRepositoryData
    {
        public RepositoryData(IForkData pull, IForkData push)
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

        public IForkData Pull { get; }
        public IForkData Push { get; }
        public string DefaultBranch { get; set; }
    }
}
