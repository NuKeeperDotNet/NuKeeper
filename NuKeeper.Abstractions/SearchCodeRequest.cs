using System.Collections.Generic;

namespace NuKeeper.Abstractions
{
    public class SearchCodeRequest
    {
        public SearchCodeRequest(string term)
        {
            Term = term;
            PerPage = 10;
            Repos = new List<KeyValuePair<string, string>>();
            SearchIn = new List<CodeInQualifier>();
        }

        public string Term { get; }
        public IList<KeyValuePair<string,string>> Repos { get; set; }
        public int PerPage { get; set; }
        public IList<CodeInQualifier> SearchIn { get; set; }
    }
}
