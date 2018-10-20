using NuKeeper.Abstract;
using Octokit;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeRequest : SearchCodeRequest, ISearchCodeRequest
    {
        private readonly string _path;

        public GithubSearchCodeRequest(string path) : base(path)
        {
            _path = path;
        }
    }
}
