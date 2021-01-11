using NuKeeper.Abstractions.Configuration;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class PullRequestRequest
    {
        public PullRequestRequest(string head, string title, string baseRef, bool deleteBranchAfterMerge, bool setAutoMerge, GitPullRequestMergeStrategy mergeStrategy)
        {
            Head = head;
            Title = title;
            DeleteBranchAfterMerge = deleteBranchAfterMerge;
            SetAutoMerge = setAutoMerge;
            MergeStrategy = mergeStrategy;

            //This can be a remote that has been passed in, this happens when run locally against a targetbranch that is remote
            BaseRef = baseRef?.Replace("origin/", string.Empty);
        }

        public string Head { get; }
        public string Title { get; }
        public string BaseRef { get; }
        public string Body { get; set; }
        public bool DeleteBranchAfterMerge { get; set; }
        public bool SetAutoMerge { get; }
        public GitPullRequestMergeStrategy MergeStrategy { get; set; }
    }
}
