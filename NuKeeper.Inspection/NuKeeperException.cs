using System;

namespace NuKeeper.Inspection
{
    [Serializable]
    public class NuKeeperException: ApplicationException
    {
        public NuKeeperException()
        {
        }

        public NuKeeperException(string message) : base(message)
        {
        }

        public NuKeeperException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
