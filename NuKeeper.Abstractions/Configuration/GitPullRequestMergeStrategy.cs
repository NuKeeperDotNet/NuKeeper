namespace NuKeeper.Abstractions.Configuration
{
    public enum GitPullRequestMergeStrategy
    {
        NoFastForward = 0,
        Rebase = 1,
        RebaseMerge = 2,
        Squash = 3
    }
}
