using System.Diagnostics.CodeAnalysis;

namespace NuKeeper.Abstract
{
    [SuppressMessage("ReSharper", "CA1717")]
    [SuppressMessage("ReSharper", "CA1724")]
    public class Permissions
    {
        public Permissions(bool admin, bool push, bool pull)
        {
            Admin = admin;
            Push = push;
            Pull = pull;
        }

        /// <summary>
        /// Whether the current user has administrative permissions
        /// </summary>
        public bool Admin { get; protected set; }

        /// <summary>
        /// Whether the current user has push permissions
        /// </summary>
        public bool Push { get; protected set; }

        /// <summary>
        /// Whether the current user has pull permissions
        /// </summary>
        public bool Pull { get; protected set; }
    }
}
