using System.Collections.Generic;

namespace NuKeeper.Abstractions.DTOs
{
    public class SearchCodeRequest
    {
        public SearchCodeRequest(string term)
        {
            Term = term;
        }

        public string Term { get; }
        public IList<(string owner, string name)> Repos { get; set; }
        public int PerPage { get; set; }
    }
}
