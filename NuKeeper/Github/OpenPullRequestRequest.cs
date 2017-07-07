namespace NuKeeper.Github
{
    public class OpenPullRequestRequest
    {
        public PullRequestData Data { get; set; }
        public string RepositoryOwner { get; set; }
        public string RepositoryName { get; set; }
    }
}