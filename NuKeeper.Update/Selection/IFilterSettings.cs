using System;
using System.Text.RegularExpressions;

namespace NuKeeper.Update.Selection
{
    public interface IFilterSettings
    {
        TimeSpan MinimumAge { get; set; }
        int MaxPackageUpdates { get; set; }
        Regex Excludes { get; set; }
        Regex Includes { get; set; }
    }
}
