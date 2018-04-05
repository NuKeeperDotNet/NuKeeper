using NuGet.Packaging.Core;
using NuKeeper.Files;

namespace NuKeeper.Engine.FilesUpdate
{
    public interface IConfigFilesUpdater
    {
        void Update(IFolder folder, PackageIdentity from, PackageIdentity to);
    }
}
