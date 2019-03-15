using NuKeeper.Inspection.RepositoryInspection;

namespace NuKeeper.EndToEndTests
{
    public class NullExclusions : IDirectoryExclusions
    {
        public bool PathIsExcluded(string path)
        {
            return false;
        }
    }
}