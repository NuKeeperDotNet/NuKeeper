using System;
using System.Collections.Generic;
using NuKeeper.Abstractions;

namespace NuKeeper.Github.Mappings
{
    public class GithubSearchCodeRequest : SearchCodeRequest
    {

        public GithubSearchCodeRequest(Octokit.SearchCodeRequest searchCodeRequest)
            : base(searchCodeRequest.Term)
        {
            foreach (var repo in searchCodeRequest.Repos)
            {
                var repoParts = repo.Split('/');
                Repos.Add(new KeyValuePair<string,string>(repoParts[0], repoParts[1]));
            }

            foreach (var item in searchCodeRequest.In)
            {
                if (Enum.TryParse<CodeInQualifier>(item.ToString(), out var parsedEnum))
                {
                    SearchIn.Add(parsedEnum);
                }
            }

            PerPage = searchCodeRequest.PerPage;
        }
    }
}
