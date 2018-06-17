using NuKeeper.Inspection.Logging;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.Inspection.Sources
{
    public class NugetConfigFileParser
    {
        private readonly INuKeeperLogger _logger;

        public NugetConfigFileParser(INuKeeperLogger logger)
        {
            _logger = logger;
        }

        public NuGetSources Parse(Stream fileContents)
        {
            XDocument xml;

            try
            {
                xml = XDocument.Load(fileContents);
            }
            catch (Exception ex)
            {
                _logger.Verbose($"failed to parse nuget.config file: {ex} {ex.Message}");
                return null;
            }

            var config = xml.Element("configuration");
            if (config == null)
            {
                return null;
            }


            var sources = config.Element("packageSources");
            if (sources == null)
            {
                return null;
            }

            var adds = sources.Elements("add");

            var values = adds
                .Select(a => a.Attribute("value")?.Value)
                .Where(v => ! string.IsNullOrWhiteSpace(v))
                .ToList();

            if (values.Count == 0)
            {
                return null;
            }

            return new NuGetSources(values);
        }
    }
}
