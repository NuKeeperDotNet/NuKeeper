using System.Collections.Generic;
using NuKeeper.Inspection.Logging;
using NuKeeper.Inspection.NuGetApi;
using NuKeeper.Inspection.Report;

#pragma warning disable CA2227

namespace NuKeeper.Configuration
{
    public class FileSettings
    {
        public string Age { get; set; }
        public string Api { get; set; }
        public string Include { get; set; }
        public string Exclude { get; set; }
        public LogLevel? Verbosity { get; set; }
        public VersionChange? Change { get; set; }

        public string IncludeRepos { get; set; }
        public string ExcludeRepos { get; set; }

        public List<string> Label { get; set; }

        public string LogFile { get; set; }

        public int? MaxPr { get; set; }
        public int? MaxRepo { get; set; }

        public bool? Consolidate { get; set; }

        public OutputFormat? OutputFormat { get; set; }
        public OutputDestination? OutputDestination { get; set; }
        public string OutputFileName { get; set; }

        public LogDestination? LogDestination { get; set; }

        public static FileSettings Empty()
        {
            return new FileSettings();
        }
    }
}
