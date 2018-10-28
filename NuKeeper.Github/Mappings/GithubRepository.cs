using System;
using NuKeeper.Abstract;

namespace NuKeeper.Github.Mappings
{
    public class GithubRepository : Octokit.Repository, IRepository
    {
        public GithubRepository(int id) : base(id)
        {

        }

        public GithubRepository(Octokit.Repository repository) : base(
         repository.Url, repository.HtmlUrl, repository.CloneUrl, repository.GitUrl, repository.SshUrl, repository.SvnUrl, repository.MirrorUrl, repository.Id, repository.NodeId, repository.Owner, repository.Name, repository.FullName, repository.Description, repository.Homepage, repository.Language, repository.Private, repository.Fork, repository.ForksCount, repository.StargazersCount, repository.DefaultBranch, repository.OpenIssuesCount, repository.PushedAt, repository.CreatedAt, repository.UpdatedAt, repository.Permissions, repository.Parent, repository.Source, repository.License, repository.HasIssues, repository.HasWiki, repository.HasDownloads, repository.HasPages, repository.SubscribersCount, repository.Size, repository.AllowRebaseMerge, repository.AllowSquashMerge, repository.AllowMergeCommit, repository.Archived)
        {

        }

        public new IRepository Parent => new GithubRepository(base.Parent);
            
        public new Permissions Permissions => new Permissions(base.Permissions.Admin, base.Permissions.Push, base.Permissions.Pull);

        public new Uri CloneUrl => new Uri(base.CloneUrl);

        public new Uri HtmlUrl => new Uri(base.HtmlUrl);

        public new User Owner => new User(base.Owner.Login, base.Owner.Name, base.Owner.Email);

    }
}
