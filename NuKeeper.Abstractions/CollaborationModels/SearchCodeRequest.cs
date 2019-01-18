using System.Collections.Generic;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class SearchCodeRequest
    {
        public SearchCodeRequest(string term)
        {
            Term = term;
        }

        public string Term { get; }
        public IList<(string owner, string name)> Repos { get; }
        public int PerPage { get; set; }
    }
}
