using System;
using System.Runtime.Serialization;

namespace NuKeeper.Abstractions
{
    [Serializable]
    public class NuKeeperException: Exception
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

        protected NuKeeperException(SerializationInfo info, StreamingContext context) : base(info, context)
        { }
    }
}
