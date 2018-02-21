using System;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    public class RespositoryBuilder
    {
        public const string ParentHtmlUrl = "http://repos.com/org/parent";
        public const string ParentCloneUrl = "http://repos.com/org/parent.git";

        public const string ForkHtmlUrl = "http://repos.com/org/repo";
        public const string ForkCloneUrl = "http://repos.com/org/repo.git";
        public const string NoMatchUrl = "http://repos.com/org/nomatch";

        public static Repository MakeRepository(bool canPull, bool canPush)
        {
            return MakeRepository(ForkHtmlUrl, ForkCloneUrl, canPull, canPush);
        }

        public static Repository MakeRepository(
            string forkHtmlUrl = ForkHtmlUrl, 
            string forkCloneUrl = ForkCloneUrl, 
            bool canPull = true, bool canPush = true)
        {
            const string omniUrl = "http://somewhere.com/fork";
            var owner = new User(omniUrl, "test user", null, 0, "test inc",
                DateTimeOffset.Now, DateTimeOffset.Now, 
                0, null, 0, 0, false, omniUrl, 1, 1,
                "testville", "testUser", "Testy",
                1, null, 0, 0,
                1, omniUrl, null, false, "test", null);


            var perms = new RepositoryPermissions(false, canPush, canPull);
            var parent = MakeParentRepo();

            return new Repository(omniUrl, forkHtmlUrl, forkCloneUrl, omniUrl, omniUrl, omniUrl, omniUrl,
                123, owner, "repoName", "repoName", "a test repo", omniUrl, "EN", false, true,
                1, 1, "master", 1, null, DateTimeOffset.Now, DateTimeOffset.Now,
                perms, parent, null, null,
                false, false, false, false, 2, 122, true, true, true);
        }

        private static Repository MakeParentRepo(
            string htmlUrl = ParentHtmlUrl,
            string cloneUrl = ParentCloneUrl)
        {
            const string omniUrl = "http://somewhere.com/parent";
            var owner = new User(omniUrl, "test user", null, 0, "test inc",
                DateTimeOffset.Now, DateTimeOffset.Now,
                0, null, 0, 0, false, omniUrl, 1, 1,
                "testville", "testUser", "Testy",
                1, null, 0, 0,
                1, omniUrl, null, false, "test", null);

            var perms = new RepositoryPermissions(false, true, true);

            return new Repository(omniUrl, htmlUrl, cloneUrl, omniUrl, omniUrl, omniUrl, omniUrl,
                123, owner, "repoName", "repoName", "a test repo", omniUrl, "EN", false, true,
                1, 1, "master", 1, null,
                DateTimeOffset.Now, DateTimeOffset.Now,
                perms, null, null, null,
                false, false, false, false, 2, 122, true, true, true);
        }
    }
}
