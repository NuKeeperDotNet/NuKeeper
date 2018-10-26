using NuKeeper.Abstract;
using Octokit;
using System;
#pragma warning disable CA1054 // Uri parameters should not be strings
namespace NuKeeper.Github.Mappings
{
    public class GithubAccount : User, IAccount
    {

        public GithubAccount(string avatarUrl, string bio, string blog, int collaborators, string company, DateTimeOffset createdAt, DateTimeOffset updatedAt, int diskUsage, string email, int followers, int following, bool? hireable, string htmlUrl, int totalPrivateRepos, int id, string location, string login, string name, string nodeId, int ownedPrivateRepos, Plan plan, int privateGists, int publicGists, int publicRepos, string url, RepositoryPermissions permissions, bool siteAdmin, string ldapDistinguishedName, DateTimeOffset? suspendedAt)
            :base(avatarUrl,bio,blog,collaborators,company,createdAt,updatedAt,diskUsage,email,followers,following,hireable,htmlUrl,totalPrivateRepos,id,location,login,name,nodeId,ownedPrivateRepos,plan,privateGists,publicGists,publicRepos,url,permissions,siteAdmin,ldapDistinguishedName,suspendedAt)
        {
            
        }

        public new string Login => base.Login;
        public new string Name => base.Name;
        public new string Email => base.Email;
    }
}
