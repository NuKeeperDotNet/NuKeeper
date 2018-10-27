using System;
using System.Text.RegularExpressions;

namespace NuKeeper.Update.Selection
{
    public class FilterSettings : IFilterSettings
    {
        public TimeSpan MinimumAge { get; set; }
        public int MaxPackageUpdates { get; set; }
        public Regex Excludes { get; set; }
        public Regex Includes { get; set; }
    }
}
