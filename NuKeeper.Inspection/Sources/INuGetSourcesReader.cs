using NuKeeper.Abstractions.Inspections.Files;
using NuKeeper.Abstractions.NuGet;

namespace NuKeeper.Inspection.Sources
{
    public interface INuGetSourcesReader
    {
        NuGetSources Read(IFolder workingFolder, NuGetSources overrideValues);
    }
}
