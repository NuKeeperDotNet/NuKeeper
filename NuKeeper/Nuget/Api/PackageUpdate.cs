using NuGet.Packaging.Core;
using NuGet.Versioning;
using NuKeeper.RepositoryInspection;

namespace NuKeeper.Nuget.Api
{
    public class PackageUpdate
    {
        public PackageUpdate(NugetPackage currentPackage, PackageIdentity newPackageIdentity)
        {
            CurrentPackage = currentPackage;
            NewPackageIdentity = newPackageIdentity;
        }

        public NugetPackage CurrentPackage { get; }
        public PackageIdentity NewPackageIdentity { get; }

        public NuGetVersion OldVersion => CurrentPackage.Version;
        public NuGetVersion NewVersion => NewPackageIdentity.Version;
    }
}