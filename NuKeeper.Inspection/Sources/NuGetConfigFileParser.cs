using NuKeeper.Inspection.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NuKeeper.Inspection.Sources
{
    public class NuGetConfigFileParser
    {
        private readonly INuKeeperLogger _logger;

        public NuGetConfigFileParser(INuKeeperLogger logger)
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

            var packageSources = PackageSourceElements(xml);
            if (packageSources == null)
            {
                _logger.Verbose("Did not find package sources in nuget.config file");
                return null;
            }

            var values = SourceValues(packageSources);

            if (values.Count == 0)
            {
                _logger.Verbose("nuget.config file contained no sources");
                return null;
            }

            return new NuGetSources(values);
        }

        private static IEnumerable<XElement> PackageSourceElements(XDocument xml)
        {
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

            return sources.Elements("add");
        }

        private static List<string> SourceValues(IEnumerable<XElement> adds)
        {
            return adds
                .Select(a => a.Attribute("value")?.Value)
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .ToList();
        }
    }
}
