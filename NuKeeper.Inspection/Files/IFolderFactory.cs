using NuKeeper.Abstractions.Inspections.Files;

namespace NuKeeper.Inspection.Files
{
    public interface IFolderFactory
    {
        void DeleteExistingTempDirs();

        IFolder FolderFromPath(string folderPath);

        IFolder UniqueTemporaryFolder();
    }
}
