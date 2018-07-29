using NuKeeper.GitHub;

namespace NuKeeper.Tests.Engine
{
    public class RepositoryBuilder
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
            bool canPull = true,
            bool canPush = true)
        {
            return new OctokitRepository(forkCloneUrl, "repoName", true, forkHtmlUrl, false,
                MakeUser(),
                new OctokitRepositoryPermissions(false, canPush, canPull),
                MakeParentRepo());
        }

        private static IRepository MakeParentRepo()
        {
            return new OctokitRepository(ParentCloneUrl, "repoName", true, ParentHtmlUrl, false,
                MakeUser(),
                new OctokitRepositoryPermissions(false, true, true),
                null);
        }

        private static IGitHubAccount MakeUser()
        {
            return new OctokitUser("testUser", "test user", "testuser@test.com");
        }
    }
}
