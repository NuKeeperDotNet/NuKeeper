using NuKeeper.Logging;

namespace NuKeeper.NuGet.Api
{
    public class PackageLookupResultReporter
    {
        private readonly INuKeeperLogger _logger;

        public PackageLookupResultReporter(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public void Report(PackageLookupResult lookupResult)
        {
            var highestVersion = lookupResult.Highest?.Identity?.Version;
            var highestMatchVersion = lookupResult.Match?.Identity?.Version;

            var allowing = lookupResult.AllowedChange == VersionChange.Major
                ? string.Empty
                : $" Allowing {lookupResult.AllowedChange} version updates.";

            if (highestVersion != null)
            {
                if (highestMatchVersion != null)
                {
                    if (highestVersion > highestMatchVersion)
                    {
                        _logger.Info($"Selected update to version {highestMatchVersion}, but version {highestVersion} is also available.{allowing}");
                    }
                    else
                    {
                       _logger.Info($"Selected update to highest version, {highestMatchVersion}.{allowing}");
                    }
                }
                else
                {
                    _logger.Info($"Version {highestVersion} is available but is not allowed.{allowing}");
                }
            }
        }
    }
}
