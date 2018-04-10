namespace NuKeeper.Inspection.Files
{
    public interface IFolderFactory
    {
        void DeleteExistingTempDirs();
        IFolder UniqueTemporaryFolder();
    }
}
