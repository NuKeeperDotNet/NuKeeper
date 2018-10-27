using System.Collections.Generic;
using System.Text.RegularExpressions;
using NuKeeper.Configuration;

namespace NuKeeper.Abstract.Configuration
{
    public class SourceControlServerSettings
    {
        public ServerScope Scope { get; set; }
        public string OrganisationName { get; set; }
        public IRepositorySettings Repository { get; set; }
        public IReadOnlyCollection<string> Labels { get; set; }
        public Regex IncludeRepos { get; set; }
        public Regex ExcludeRepos { get; set; }
    }
}
