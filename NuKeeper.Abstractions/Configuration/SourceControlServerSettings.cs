using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace NuKeeper.Abstractions.Configuration
{
    public class SourceControlServerSettings
    {
        public ServerScope Scope { get; set; }
        public string OrganisationName { get; set; }
        public RepositorySettings Repository { get; set; }
        public IReadOnlyCollection<string> Labels { get; set; }
        public IReadOnlyCollection<string> Reviewers { get; set; }
        public Regex IncludeRepos { get; set; }
        public Regex ExcludeRepos { get; set; }
    }
}
