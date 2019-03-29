namespace NuKeeper.Abstractions.CollaborationModels
{
    public class PullRequestRequest
    {
        public PullRequestRequest(string head, string title, string baseRef)
        {
            Head = head;
            Title = title;
            BaseRef = baseRef.Replace("origin/", string.Empty);
        }

        public string Head { get; }
        public string Title { get; }
        public string BaseRef { get; }
        public string Body { get; set; }
    }
}
