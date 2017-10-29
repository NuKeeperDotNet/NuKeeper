namespace NuKeeper.Configuration
{
    public class ModalSettings
    {
        public GithubMode Mode { get; set;  }
        public string OrganisationName { get; set; }
        public RepositorySettings Repository { get; set; }
    }
}