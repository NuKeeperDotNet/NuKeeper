using NuKeeper.RepositoryInspection;

namespace NuKeeper.Engine
{
    public static class BranchNamer
    {
        public static string MakeName(PackageUpdateSet updateSet)
        {
            return $"nukeeper-update-{updateSet.SelectedId}-to-{updateSet.SelectedVersion}";
        }
    }
}
