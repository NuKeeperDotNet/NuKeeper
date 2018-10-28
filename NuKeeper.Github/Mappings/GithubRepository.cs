using System;
using System.Diagnostics.CodeAnalysis;
using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubRepository : Repository, IRepository
    {
        public GithubRepository(int id) : base(id)
        {

        }

        [SuppressMessage("ReSharper", "CA1054")]
        public GithubRepository(string url, string htmlUrl, string cloneUrl, string gitUrl, string sshUrl, string svnUrl, string mirrorUrl, long id, string nodeId, Octokit.User owner, string name, string fullName, string description, string homepage, string language, bool @private, bool fork, int forksCount, int stargazersCount, string defaultBranch, int openIssuesCount, DateTimeOffset? pushedAt, DateTimeOffset createdAt, DateTimeOffset updatedAt, RepositoryPermissions permissions, Repository parent, Repository source, LicenseMetadata license, bool hasIssues, bool hasWiki, bool hasDownloads, bool hasPages, int subscribersCount, long size, bool? allowRebaseMerge, bool? allowSquashMerge, bool? allowMergeCommit, bool archived) : base(
            url, htmlUrl, cloneUrl, gitUrl, sshUrl, svnUrl, mirrorUrl, id, nodeId, owner, name, fullName, description, homepage, language, @private, fork, forksCount, stargazersCount, defaultBranch, openIssuesCount, pushedAt, createdAt, updatedAt, permissions, parent, source, license, hasIssues, hasWiki, hasDownloads, hasPages, subscribersCount, size, allowRebaseMerge, allowSquashMerge, allowMergeCommit, archived)
        {

        }

        public new IRepository Parent => AutoMapperConfiguration.GithubMappingConfiguration.Map<GithubRepository>(base.Parent);

        public new Permissions Permissions => AutoMapperConfiguration.GithubMappingConfiguration.Map<Permissions>(base.Permissions);

        public new Uri CloneUrl => new Uri(base.CloneUrl);

        public new Uri HtmlUrl => new Uri(base.HtmlUrl);

        public new Abstract.User Owner => new Abstract.User(base.Owner.Login, base.Owner.Name, base.Owner.Email);

    }
}
