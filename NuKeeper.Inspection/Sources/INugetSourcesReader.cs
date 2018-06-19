using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public interface INugetSourcesReader
    {
        NuGetSources Read(IFolder workingFolder);
    }
}
