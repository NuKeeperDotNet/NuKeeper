using System.Collections.Generic;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class SearchCodeRequest
    {
        public SearchCodeRequest(string term, IReadOnlyCollection<(string owner, string name)> repos)
        {
            Term = term;
            Repos = repos;
        }

        public string Term { get; }
        public IReadOnlyCollection<(string owner, string name)> Repos { get; }
        public int PerPage { get; set; }
    }
}
