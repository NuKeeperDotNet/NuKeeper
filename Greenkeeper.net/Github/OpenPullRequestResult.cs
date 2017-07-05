using System.Net.Http;

namespace NuKeeper.Github
{
    public class OpenPullRequestResult
    {
        public OpenPullRequestResult(HttpResponseMessage response)
        {
            Response = response;
        }
        public HttpResponseMessage Response { get; }
    }
}