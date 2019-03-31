namespace NuKeeper.Abstractions.CollaborationModels
{
    public class PullRequestRequest
    {
        public PullRequestRequest(string head, string title, string baseRef)
        {
            Head = head;
            Title = title;

            //This can be a remote that has been passed in, this happens when run locally against a targetbranch that is remote
            BaseRef = baseRef.Contains("origin/") ? baseRef.Replace("origin/", string.Empty) : baseRef;
        }

        public string Head { get; }
        public string Title { get; }
        public string BaseRef { get; }
        public string Body { get; set; }
    }
}
