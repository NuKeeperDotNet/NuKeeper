using System;
using System.Text.RegularExpressions;

namespace NuKeeper.Update.Selection
{
    public interface IFilterSettings
    {
        Regex Excludes { get; set; }
        Regex Includes { get; set; }
        int MaxPackageUpdates { get; set; }
        TimeSpan MinimumAge { get; set; }
    }
}
