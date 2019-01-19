using System.Collections.Generic;
using System.Linq;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class SearchRepo
    {
        public SearchRepo(string owner, string name)
        {
            Owner = owner;
            Name = name;
        }

        public string Owner { get; }
        public string Name { get; }
    }

    public class SearchCodeRequest
    {
        public SearchCodeRequest(string term, IEnumerable<SearchRepo> repos)
        {
            Term = term;
            Repos = repos.ToList();
        }

        public string Term { get; }
        public IReadOnlyCollection<SearchRepo> Repos { get; }
        public int PerPage { get; set; }
    }
}
