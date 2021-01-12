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
        public SearchCodeRequest(IEnumerable<SearchRepo> repos, string term, IEnumerable<string> extensions)
        {
            Term = term;
            Extensions = extensions;
            Repos = repos.ToList();
        }

        public string Term { get; }
        public IEnumerable<string> Extensions { get; }
        public IReadOnlyCollection<SearchRepo> Repos { get; }
        public int PerPage { get; set; }
    }
}
