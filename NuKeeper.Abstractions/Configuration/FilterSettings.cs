using System;
using System.Text.RegularExpressions;

namespace NuKeeper.Abstractions.Configuration
{
    public class FilterSettings
    {
        public TimeSpan MinimumAge { get; set; }
        public int MaxPackageUpdates { get; set; }
        public Regex Excludes { get; set; }
        public Regex Includes { get; set; }
        public bool IncludeVersion { get; set; }
    }
}
