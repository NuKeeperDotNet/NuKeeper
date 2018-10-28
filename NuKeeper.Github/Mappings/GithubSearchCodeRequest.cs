using System;
using System.Collections.Generic;
using NuKeeper.Abstractions;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeRequest : ISearchCodeRequest
    {
        private readonly Octokit.SearchCodeRequest _searchCodeRequest;

        public GithubSearchCodeRequest(Octokit.SearchCodeRequest searchCodeRequest)
        {
            _searchCodeRequest = searchCodeRequest;
        }

        public string Term => _searchCodeRequest.Term;

        public int PerPage => _searchCodeRequest.PerPage;

        public IList<KeyValuePair<string, string>> Repos
        {
            get
            {
                var repos = _searchCodeRequest.Repos;
                var returnList = new List<KeyValuePair<string, string>>();
                foreach (var repo in repos)
                {
                    var repoParts = repo.Split('/');
                    returnList.Add(new KeyValuePair<string,string>(repoParts[0], repoParts[1]));
                }
                return returnList;
            }
        }

        public IList<CodeInQualifier> SearchIn
        {
            get
            {
                var returnList = new List<CodeInQualifier>();
                foreach (var item in _searchCodeRequest.In)
                {
                    if (Enum.TryParse<CodeInQualifier>(item.ToString(), out var parsedEnum))
                    {
                        returnList.Add(parsedEnum);
                    }
                }
                return returnList;               
            }
        }
    }
}
