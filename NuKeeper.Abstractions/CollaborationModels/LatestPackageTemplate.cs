namespace NuKeeper.Abstractions.CollaborationModels
{
    public class LatestPackageTemplate
    {
        public string Version { get; set; }
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
        public PublicationTemplate Publication { get; set; }
    }
}
