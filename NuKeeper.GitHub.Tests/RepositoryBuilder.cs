using System;
using NuKeeper.Abstractions.CollaborationModels;

#pragma warning disable CA1054

namespace NuKeeper.GitHub.Tests
{
    public static class RepositoryBuilder
    {
        public const string ParentCloneUrl = "http://repos.com/org/parent.git";

        public const string ForkCloneUrl = "http://repos.com/org/repo.git";
        public const string NoMatchUrl = "http://repos.com/org/nomatch";

        public static Repository MakeRepository(bool canPull, bool canPush)
        {
            return MakeRepository(ForkCloneUrl, canPull, canPush);
        }

        public static Repository MakeRepository(
            string forkCloneUrl = ForkCloneUrl,
            bool canPull = true,
            bool canPush = true,
            string name = "repoName")
        {
            return new Repository(
                name,
                false,
                new UserPermissions(false, canPush, canPull),
                new Uri(forkCloneUrl),
                MakeUser(),
                true,
                MakeParentRepo());
        }

        private static Repository MakeParentRepo()
        {
            return new Repository(
                "repoName",
                false,
                new UserPermissions(false, true, true),
                new Uri(ParentCloneUrl),
                MakeUser(),
                true,
                null
            );
        }

        public static User MakeUser()
        {
            return new User("testUser", "Testy", "testuser@test.com");
        }
    }
}
