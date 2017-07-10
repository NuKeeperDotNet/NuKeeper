using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.NuGet.Api
{
    public class PackageUpdate
    {
        public PackageUpdate(PackageInProject currentPackage, PackageIdentity newPackageIdentity)
        {
            CurrentPackage = currentPackage;
            NewPackageIdentity = newPackageIdentity;
        }

        public PackageInProject CurrentPackage { get; }
        public PackageIdentity NewPackageIdentity { get; }

        public string PackageId => CurrentPackage.Id;

        public NuGetVersion OldVersion => CurrentPackage.Version;
        public NuGetVersion NewVersion => NewPackageIdentity.Version;
    }
}