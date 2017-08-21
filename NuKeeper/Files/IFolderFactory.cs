namespace NuKeeper.Files
{
    public interface IFolderFactory
    {
        void DeleteExistingTempDirs();
        IFolder UniqueTemporaryFolder();
    }
}