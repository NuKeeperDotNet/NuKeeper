using System.IO;
using System.Linq;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public class NugetConfigFileReader
    {
        private NugetConfigFileParser _parser;

        public NugetConfigFileReader(NugetConfigFileParser parser)
        {
            _parser = parser;
        }

        public NuGetSources ReadNugetSources(IFolder workingFolder)
        {
            var configFile = workingFolder.Find("nuget.config")
                .FirstOrDefault();

            if (configFile == null)
            {
                return null;
            }

            using (var fileContents = File.OpenRead(configFile.FullName))
            {
                return _parser.Parse(fileContents);
            }
        }
    }
}
