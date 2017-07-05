namespace NuKeeper.Github
{
    public class OpenPullRequestRequest
    {
        public string Title { get; set; }
        public string Body { get; set; }
        public string HeadBranch { get; set; }
        public string BaseBranch { get; set; }
    }
}