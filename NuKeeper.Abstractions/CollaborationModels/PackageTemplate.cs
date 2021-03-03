#pragma warning disable CA1819 // Properties should not return arrays
#pragma warning disable CA1056 // Uri properties should not be strings

using System;
using System.Linq;

namespace NuKeeper.Abstractions.CollaborationModels
{
    public class PackageTemplate
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string FromVersion => MultipleUpdates ?
            string.Empty
            : Updates.FirstOrDefault()?.FromVersion;
        public string AllowedChange { get; set; }
        public string ActualChange { get; set; }
        public PublicationTemplate Publication { get; set; }
        public int ProjectsUpdated =>
            Updates?.Length ?? 0;
        public LatestPackageTemplate LatestVersion { get; set; }
        public string Url { get; set; }
        public string SourceUrl { get; set; }
        public bool IsFromNuget { get; set; }
        public UpdateTemplate[] Updates { get; set; }
        public bool MultipleProjectsUpdated => Updates?.Length > 1;
        public bool MultipleUpdates => Updates?
            .Select(u => u.FromVersion)
            .Distinct(StringComparer.InvariantCultureIgnoreCase)
            .Count() > 1;
    }
}
