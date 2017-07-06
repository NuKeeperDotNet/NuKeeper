namespace NuKeeper.Github
{
    public class PullRequestData
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string Head { get; set; }
        public string Base { get; set; }
    }

    public class OpenPullRequestRequest
    {
        public PullRequestData Data { get; set; }
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}