namespace NuKeeper.Configuration
{
    public class ModalSettings
    {
        public RunMode Mode { get; set;  }
        public string OrganisationName { get; set; }
        public RepositorySettings Repository { get; set; }
        public string[] Labels { get; set; }
    }
}
