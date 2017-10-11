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
            if (highestVersion == null)
            {
                return;
            }

            var allowing = lookupResult.AllowedChange == VersionChange.Major
                ? string.Empty
                : $" Allowing {lookupResult.AllowedChange} version updates.";

            var highestMatchVersion = lookupResult.Match?.Identity?.Version;

            var packageId = lookupResult.Highest.Identity.Id;

            if (highestMatchVersion == null)
            {
                _logger.Info($"Package {packageId} version {highestVersion} is available but is not allowed.{allowing}");
                return;
            }

            if (highestVersion > highestMatchVersion)
            {
                _logger.Info($"Selected update of package {packageId} to version {highestMatchVersion}, but version {highestVersion} is also available.{allowing}");
            }
            else
            {
                _logger.Info($"Selected update of package {packageId} to highest version, {highestMatchVersion}.{allowing}");
            }
        }
    }
}
