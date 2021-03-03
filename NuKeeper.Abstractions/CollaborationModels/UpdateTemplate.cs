namespace NuKeeper.Abstractions.CollaborationModels
{
    public class UpdateTemplate
    {
        public string SourceFilePath { get; set; }
        public string FromVersion { get; set; }
#pragma warning disable CA1056 // Uri properties should not be strings
        public string FromUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
        public string ToVersion { get; set; }
    }
}
