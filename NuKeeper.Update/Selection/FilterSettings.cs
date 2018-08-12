using System;
using System.Text.RegularExpressions;

namespace NuKeeper.Update.Selection
{
    public class FilterSettings
    {
        public TimeSpan MinimumAge { get; set; }
        public int MaxUpdates { get; set; }
        public Regex Excludes { get; set; }
        public Regex Includes { get; set; }
    }
}
