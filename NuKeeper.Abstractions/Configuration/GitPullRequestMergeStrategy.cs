namespace NuKeeper.Abstractions.Configuration
{
    public enum GitPullRequestMergeStrategy
    {
        noFastForward,
        rebase,
        rebaseMerge,
        squash
    }
}
