using System;
using NuKeeper.Abstract;
using NuKeeper.Github.Mappings;
using Octokit;

#pragma warning disable CA1054

namespace NuKeeper.Tests.Engine
{
    public static class RepositoryBuilder
    {
        public const string ParentHtmlUrl = "http://repos.com/org/parent";
        public const string ParentCloneUrl = "http://repos.com/org/parent.git";

        public const string ForkHtmlUrl = "http://repos.com/org/repo";
        public const string ForkCloneUrl = "http://repos.com/org/repo.git";
        public const string NoMatchUrl = "http://repos.com/org/nomatch";

        public static IRepository MakeRepository(bool canPull, bool canPush)
        {
            return MakeRepository(ForkHtmlUrl, ForkCloneUrl, canPull, canPush);
        }

        public static IRepository MakeRepository(
            string forkHtmlUrl = ForkHtmlUrl,
            string forkCloneUrl = ForkCloneUrl,
            bool canPull = true, bool canPush = true,
            string name = "repoName")
        {
            const string omniUrl = "http://somewhere.com/fork";
            var owner = MakeUser(omniUrl);

            var perms = new Octokit.RepositoryPermissions(false, canPush, canPull);
            var parent = MakeParentRepo();

            var repo = new Repository(omniUrl, forkHtmlUrl, forkCloneUrl,
                omniUrl, omniUrl, omniUrl, omniUrl,
                123, "nodeId",
                owner, name, name, "a test repo", "homepage",
                "EN", false, true,
                1, 1, "master",
                1, null, DateTimeOffset.Now, DateTimeOffset.Now,
                perms, parent, null, null,
                false, false, false, false,
                2, 122, true, true, true, false);

            return new GithubRepository(repo);
        }

        private static Repository MakeParentRepo(
            string htmlUrl = ParentHtmlUrl,
            string cloneUrl = ParentCloneUrl)
        {
            const string omniUrl = "http://somewhere.com/parent";
            var owner = MakeUser(omniUrl);

            var perms = new Octokit.RepositoryPermissions(false, true, true);

            var repo = new Repository(omniUrl, htmlUrl, cloneUrl,
                omniUrl, omniUrl, omniUrl, omniUrl,
                123, "nodeId",
                owner, "repoName", "repoName", "a test repo", omniUrl, "EN", false, true,
                1, 1, "master", 1, null,
                DateTimeOffset.Now, DateTimeOffset.Now,
                perms, null, null, null,
                false, false, false, false, 2, 122, true, true, true, false);

            return repo;
        }

        public static Octokit.User MakeUser(string omniUrl)
        {
            return new Octokit.User(omniUrl, "test user", null, 0, "test inc",
                DateTimeOffset.Now, DateTimeOffset.Now,
                0, "testuser@test.com", 0, 0,
                false, omniUrl, 1, 1,
                "testville", "testUser", "Testy",
                "nodeId",
                1, null, 0, 0,
                1, omniUrl, null, false, "test", null);
        }
    }
}
