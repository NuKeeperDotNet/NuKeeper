using NuKeeper.Inspection.Files;

namespace NuKeeper.Inspection.Sources
{
    public interface INugetSourcesFactory
    {
        NuGetSources ReadNugetSources(IFolder workingFolder);
    }
}
