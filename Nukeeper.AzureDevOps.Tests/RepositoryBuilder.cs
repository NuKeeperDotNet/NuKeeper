using System;
using NuKeeper.Abstractions.CollaborationModels;

#pragma warning disable CA1054

namespace Nukeeper.AzureDevOps.Tests
{
    public static class RepositoryBuilder
    {
#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static Uri ParentHtmlUrl = new Uri("http://repos.com/org/parent");
        public static Uri ParentCloneUrl = new Uri("http://repos.com/org/parent.git");

        public static Uri ForkHtmlUrl = new Uri("http://repos.com/org/repo");
        public static Uri ForkCloneUrl = new Uri("http://repos.com/org/repo.git");
        public static Uri NoMatchUrl = new Uri("http://repos.com/org/nomatch");

        public static Repository MakeRepository(bool canPull, bool canPush)
        {
            return MakeRepository(ForkCloneUrl, canPull, canPush);
        }

        public static Repository MakeRepository(
            Uri forkCloneUrl = null,
            bool canPull = true,
            bool canPush = true,
            string name = "repoName")
        {
            return new Repository(
                name,
                false,
                new UserPermissions(false, canPush, canPull),
                forkCloneUrl ?? ForkCloneUrl,
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
                ParentCloneUrl,
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
