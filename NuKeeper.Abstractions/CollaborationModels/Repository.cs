using System;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class Repository
    {
        public Repository(string name, bool archived, UserPermissions userPermissions, Uri htmlUrl, Uri cloneUrl, User owner, bool fork, Repository parent)
        {
            Name = name;
            Archived = archived;
            UserPermissions = userPermissions;
            HtmlUrl = htmlUrl;
            CloneUrl = cloneUrl;
            Owner = owner;
            Fork = fork;
            Parent = parent;
        }

        public string Name { get; }
        public bool Archived { get; }
        public UserPermissions UserPermissions { get; }
        public Uri HtmlUrl { get; }
        public User Owner { get; }
        public bool Fork { get; }
        public Repository Parent { get; }
        public Uri CloneUrl { get; set; }
    }
}
