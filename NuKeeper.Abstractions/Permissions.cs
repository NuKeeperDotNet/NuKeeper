namespace NuKeeper.Abstractions
{
    public class RepositoryPermissions
    {
        public RepositoryPermissions(bool admin, bool push, bool pull)
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
