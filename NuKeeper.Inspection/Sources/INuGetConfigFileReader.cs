using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Inspection.Sources
{
    public interface INuGetConfigFileReader
    {
        NuGetSources ReadNugetSources(IFolder workingFolder);
    }
}
