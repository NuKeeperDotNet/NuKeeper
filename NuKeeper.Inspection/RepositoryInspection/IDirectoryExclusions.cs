namespace NuKeeper.Inspection.RepositoryInspection
{
    public interface IDirectoryExclusions
    {
        bool PathIsExcluded(string path);
    }
}
