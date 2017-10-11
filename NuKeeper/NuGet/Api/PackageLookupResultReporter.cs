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
            var highestMatchVersion = lookupResult.Highest?.Identity?.Version;

            if (highestVersion != null)
            {
                if (highestMatchVersion != null)
                {
                    if (highestVersion > highestMatchVersion)
                    {
                        _logger.Info($"Allowing {lookupResult.AllowedChange} updates, selected update to version {highestMatchVersion}, but version {highestVersion} is also available.");
                    }
                    else
                    {
                        _logger.Info($"Allowing {lookupResult.AllowedChange} updates, selected update to higest version {highestMatchVersion}.");

                    }
                }
                else
                {
                    // There is a highest version but no match
                    _logger.Info($"Allowing {lookupResult.AllowedChange} updates, there is no matching update, but  {highestVersion} is also available.");
                }
            }
        }
    }
}
