namespace NuKeeper.Types.Files
{
    public interface IFolderFactory
    {
        void DeleteExistingTempDirs();
        IFolder UniqueTemporaryFolder();
    }
}
