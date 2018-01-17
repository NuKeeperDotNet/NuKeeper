using System;
using Octokit;

namespace NuKeeper.Tests.Engine
{
    public class RespositoryBuilder
    {
        public const string ParentUrl = "http://repos.com/org/parent";
        public const string ForkUrl = "http://repos.com/org/repo";
        public const string NoMatchUrl = "http://repos.com/org/nomatch";

        public static Repository MakeRepository()
        {
            const string omniUrl = "http://somewhere.com/fork";
            var owner = new User(omniUrl, "test user", null, 0, "test inc",
                DateTimeOffset.Now, 0, null, 0, 0, false, omniUrl, 1, 1,
                "testville", "testUser", "Testy",
                1, null, 0, 0,
                1, omniUrl, null, false, "test", null);


            var perms = new RepositoryPermissions(false, true, true);
            var parent = MakeParentRepo();

            return new Repository(omniUrl, ForkUrl, omniUrl, omniUrl, omniUrl, omniUrl, omniUrl,
                123, owner, "repoName", "repoName", "a test repo", omniUrl, "EN", false, true,
                1, 1, "master", 1, null, DateTimeOffset.Now, DateTimeOffset.Now,
                perms, parent,
                null, false, false, false, false, 2, 122, true, true, true);
        }

        public static Repository MakeParentRepo()
        {
            const string omniUrl = "http://somewhere.com/parent";
            var owner = new User(omniUrl, "test user", null, 0, "test inc",
                DateTimeOffset.Now, 0, null, 0, 0, false, omniUrl, 1, 1,
                "testville", "testUser", "Testy",
                1, null, 0, 0,
                1, omniUrl, null, false, "test", null);


            var perms = new RepositoryPermissions(false, true, true);

            return new Repository(omniUrl, ParentUrl, omniUrl, omniUrl, omniUrl, omniUrl, omniUrl,
                123, owner, "repoName", "repoName", "a test repo", omniUrl, "EN", false, true,
                1, 1, "master", 1, null, DateTimeOffset.Now, DateTimeOffset.Now, perms, null,
                null, false, false, false, false, 2, 122, true, true, true);
        }
    }
}