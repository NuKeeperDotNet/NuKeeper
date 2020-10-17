namespace NuKeeper.Abstractions.CollaborationModels
{
    public class FooterTemplate
    {
#pragma warning disable CA1056 // Uri properties should not be strings
        public string NuKeeperUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
        public string WarningMessage { get; set; }
    }
}
