using System;
using System.Threading.Tasks;
using NuGet.Packaging.Core;
using NuKeeper.Abstractions;
using NuKeeper.Abstractions.Configuration;
using NuKeeper.Abstractions.NuGet;
using NuKeeper.Abstractions.NuGetApi;

namespace NuKeeper.Inspection.NuGetApi
{
    public class ApiPackageLookup : IApiPackageLookup
    {
        private readonly IPackageVersionsLookup _packageVersionsLookup;

        public ApiPackageLookup(IPackageVersionsLookup packageVersionsLookup)
        {
            _packageVersionsLookup = packageVersionsLookup;
        }

        public async Task<PackageLookupResult> FindVersionUpdate(
            PackageIdentity package,
            NuGetSources sources,
            VersionChange allowedChange,
            UsePrerelease usePrerelease,
            bool throwOnGitError)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var includePrerelease = ShouldAllowPrerelease(package, usePrerelease);

            var foundVersions = await _packageVersionsLookup.Lookup(package.Id, includePrerelease, throwOnGitError, sources);
            return VersionChanges.MakeVersions(package.Version, foundVersions, allowedChange);
        }

        private static bool ShouldAllowPrerelease(PackageIdentity package, UsePrerelease usePrerelease)
        {
            switch (usePrerelease)
            {
                case UsePrerelease.Always:
                    return true;

                case UsePrerelease.Never:
                    return false;

                case UsePrerelease.FromPrerelease:
                    return package.Version.IsPrerelease;

                default:
                    throw new NuKeeperException($"Invalid UsePrerelease value: {usePrerelease}");
            }
        }
    }
}
