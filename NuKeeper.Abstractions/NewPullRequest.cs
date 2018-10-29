namespace NuKeeper.Abstractions
{
    public class NewPullRequest
    {
        public NewPullRequest(string title, string head, string baseRef)
        {
            Title = title;
            Head = head;
            BaseRef = baseRef;
        }

        public string Title { get; }
        public string Head { get; }
        public string BaseRef { get; }
    }
}
