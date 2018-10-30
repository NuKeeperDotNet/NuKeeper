using NuKeeper.Abstractions.NuGet;
using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public interface INuGetSourcesReader
    {
        NuGetSources Read(IFolder workingFolder, NuGetSources overrideValues);
    }
}
